using Newtonsoft.Json;
using NLog;
using System.Collections.ObjectModel;
using System.Text;
using ZigbeeBridgeAddon.SerialClient;
using ZigbeeBridgeAddon.SerialClient.Enums;
using ZigbeeBridgeAddon.SerialClient.Events;
using ZigbeeBridgeAddon.SerialClient.Models;
using ZigbeeBridgeAddon.SerialClient.Models.Commands;

namespace ZigbeeBridgeAddon.Services
{
    public class SerialClientService : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SerialPortClient _portClient;
        public ObservableCollection<SerialMessage> Messages { get; private set; } = [];

        public SerialClientService(SerialSettings settings)
        {
            _portClient = new SerialPortClient();
            _portClient.SetPort(settings.Port);
            _portClient.ConnectionStatusChanged += ConnectionStatusChanged;
            _portClient.MessageReceived += MessageReceived;
            _portClient.Connect();
        }

        public void ClearLog()
        {
            Messages.Clear();
        }

        private void ConnectionStatusChanged(object sender, ConnectionStatusChangedEvent args)
        {
            _logger.Debug("Connected = {0}", args.Connected);
        }

        private void MessageReceived(object sender, MessageReceivedEvent args)
        {
            var json = Encoding.UTF8.GetString(args.Data, 0, args.Data.Length);
            if (json.StartsWith("{\"type\""))
            {
                var message = JsonConvert.DeserializeObject<BaseMessage>(json, new BaseMessageConverter());
                if (message != null)
                {
                    Messages.Add((SerialMessage)message);
                }
            } 
            else
            {
                Messages.Add(new SerialMessage(MessageType.Log, json));
            }
            _logger.Debug("Received message: {0}", json);
        }

        public void SendRebootCommand()
        {
            var command = new BaseCommand(MessageType.Reboot);
            var json = JsonConvert.SerializeObject(command);
            _portClient.SendMessage(Encoding.UTF8.GetBytes(json));
        }

        public void SendInitCommand(IEnumerable<ZigbeeObject> data)
        {
            var command = new InitCommand(MessageType.Init, data);
            var json = JsonConvert.SerializeObject(command);
            _portClient.SendMessage(Encoding.UTF8.GetBytes(json));
        }

        public void Dispose()
        {
            _portClient.ConnectionStatusChanged -= ConnectionStatusChanged;
            _portClient.MessageReceived -= MessageReceived;
            _portClient.Disconnect();
            GC.SuppressFinalize(this);
        }
    }
}
