using NLog;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using ZigbeeBridgeAddon.SerialClient.Enums;
using ZigbeeBridgeAddon.SerialClient.Events;

namespace ZigbeeBridgeAddon.SerialClient
{
    public class SerialPortClient
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private SerialPort SerialPort { get; set; } = null!;

        private string _portName = "";
        private int _baudRate = 115200;
        private StopBits _stopBits = StopBits.One;
        private Parity _parity = Parity.None;
        private DataBits _dataBits = DataBits.Eight;

        // Read/Write error state variable
        private bool _gotReadWriteError = true;

        // Serial port reader task
        private Thread? _reader;
        private CancellationTokenSource _readerCts;
        // Serial port connection watcher
        private Thread? _connectionWatcher;
        private CancellationTokenSource _connectionWatcherCts;

        private readonly Lock _accessLock = new();
        private bool _disconnectRequested;


        public delegate void ConnectionStatusChangedEventHandler(object sender, ConnectionStatusChangedEvent args);

        public event ConnectionStatusChangedEventHandler ConnectionStatusChanged;

        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEvent args);

        public event MessageReceivedEventHandler MessageReceived;

        public int ReconnectDelay { get; set; } = 1000;

        public SerialPortClient()
        {
            _connectionWatcherCts = new CancellationTokenSource();
            _readerCts = new CancellationTokenSource();
        }

        /// <summary>
        /// Connect to the serial port.
        /// </summary>
        public bool Connect()
        {
            if (_disconnectRequested)
            {
                return false;
            }
            lock (_accessLock)
            {
                Disconnect();
                Open();
                _connectionWatcherCts = new CancellationTokenSource();
                _connectionWatcher = new Thread(ConnectionWatcherTask) { IsBackground = true };
                _connectionWatcher.Start(_connectionWatcherCts.Token);
            }
            return IsConnected;
        }

        /// <summary>
        /// Disconnect the serial port.
        /// </summary>
        public void Disconnect()
        {
            if (_disconnectRequested)
            {
                return;
            }
            _disconnectRequested = true;
            Close();
            lock (_accessLock)
            {
                if (_connectionWatcher != null)
                {
                    if (!_connectionWatcher.Join(5000))
                    {
                        _connectionWatcherCts.Cancel();
                    }
                    _connectionWatcher = null;
                }
                _disconnectRequested = false;
            }
        }

        public bool IsConnected
        {
            get { return SerialPort != null && !_gotReadWriteError && !_disconnectRequested; }
        }

        public void SetPort(string portName, int baudRate = 115200, StopBits stopBits = StopBits.One, Parity parity = Parity.None, DataBits dataBits = DataBits.Eight)
        {
            if (_portName != portName || _baudRate != baudRate || stopBits != _stopBits || parity != _parity || dataBits != _dataBits)
            {
                _portName = portName;
                _baudRate = baudRate;
                _stopBits = stopBits;
                _parity = parity;
                _dataBits = dataBits;
                if (IsConnected)
                {
                    Connect();
                }
                LogDebug(string.Format("Port parameters changed (port name {0} / baudrate {1} / stopbits {2} / parity {3} / databits {4})", portName, baudRate, stopBits, parity, dataBits));
            }
        }

        public bool SendMessage(byte[] message)
        {
            bool success = false;
            if (IsConnected)
            {
                try
                {
                    SerialPort.Write(message, 0, message.Length);
                    success = true;
                    LogDebug(Encoding.UTF8.GetString(message));
                }
                catch (Exception e)
                {
                    LogError(e);
                }
            }
            return success;
        }

        private bool Open()
        {
            bool success = false;
            lock (_accessLock)
            {
                Close();
                try
                {
                    bool tryOpen = true;

                    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    if (!isWindows)
                    {
                        tryOpen = tryOpen && File.Exists(_portName);
                    }
                    if (tryOpen)
                    {
                        SerialPort = new SerialPort();
                        SerialPort.ErrorReceived += HandleErrorReceived;
                        SerialPort.PortName = _portName;
                        SerialPort.BaudRate = _baudRate;
                        SerialPort.StopBits = _stopBits;
                        SerialPort.Parity = _parity;
                        SerialPort.DataBits = (int)_dataBits;

                        SerialPort.Open();
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    LogError(e);
                    Close();
                }
                if (SerialPort != null && SerialPort.IsOpen)
                {
                    _gotReadWriteError = false;
                    _readerCts = new CancellationTokenSource();
                    _reader = new Thread(ReaderTask) { IsBackground = true };
                    _reader.Start(_readerCts.Token);
                    OnConnectionStatusChanged(new ConnectionStatusChangedEvent(true));
                }
            }
            return success;
        }

        private void Close()
        {
            lock (_accessLock)
            {
                // Stop the Reader task
                if (_reader != null)
                {
                    if (!_reader.Join(5000))
                        _readerCts.Cancel();
                    _reader = null;
                }
                if (SerialPort != null)
                {
                    SerialPort.ErrorReceived -= HandleErrorReceived;
                    if (SerialPort.IsOpen)
                    {
                        SerialPort.Close();
                        OnConnectionStatusChanged(new ConnectionStatusChangedEvent(false));
                    }
                    SerialPort.Dispose();
                }
                _gotReadWriteError = true;
            }
        }

        private void HandleErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            LogError(e.EventType);
        }

        private void ReaderTask(object data)
        {
            var ct = (CancellationToken)data;
            while (IsConnected && !ct.IsCancellationRequested)
            {
                try
                {
                    int msglen = SerialPort.BytesToRead;
                    if (msglen > 0)
                    {
                        byte[] message = new byte[msglen];
                        int readBytes = 0;
                        while (readBytes <= 0)
                            readBytes = SerialPort.Read(message, readBytes, msglen - readBytes); // noop
                        if (MessageReceived != null)
                        {
                            OnMessageReceived(new MessageReceivedEvent(message));
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception e)
                {
                    LogError(e);
                    _gotReadWriteError = true;
                    Thread.Sleep(1000);
                }
            }
        }

        private void ConnectionWatcherTask(object data)
        {
            var ct = (CancellationToken)data;
            while (!_disconnectRequested && !ct.IsCancellationRequested)
            {
                if (_gotReadWriteError)
                {
                    try
                    {
                        Close();
                        Thread.Sleep(ReconnectDelay);
                        if (!_disconnectRequested)
                        {
                            try
                            {
                                Open();
                            }
                            catch (Exception e)
                            {
                                LogError(e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogError(e);
                    }
                }
                if (!_disconnectRequested)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void LogDebug(string message)
        {
            _logger.Debug(message);
        }

        private void LogError(Exception ex)
        {
            _logger.Error(ex, null);
        }

        private void LogError(SerialError error)
        {
            _logger.Error("SerialPort ErrorReceived: {0}", error);
        }

        protected virtual void OnConnectionStatusChanged(ConnectionStatusChangedEvent args)
        {
            LogDebug(args.Connected.ToString());
            ConnectionStatusChanged?.Invoke(this, args);
        }

        protected virtual void OnMessageReceived(MessageReceivedEvent args)
        {
            LogDebug(Encoding.UTF8.GetString(args.Data));
            MessageReceived?.Invoke(this, args);
        }
    }
}
