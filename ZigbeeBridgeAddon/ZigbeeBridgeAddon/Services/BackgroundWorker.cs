using NLog;
using System.Collections.Specialized;
using ZigbeeBridgeAddon.SerialClient.Enums;
using ZigbeeBridgeAddon.SerialClient.Models;

namespace ZigbeeBridgeAddon.Services
{
    public class BackgroundWorker(IServiceProvider provider, SerialClientService serialService) : BackgroundService, IAsyncDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private DBService _dbService;
        private HAService _haService;
        private AsyncServiceScope _scope;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _scope = provider.CreateAsyncScope();
            _haService = _scope.ServiceProvider.GetRequiredService<HAService>();
            _dbService = _scope.ServiceProvider.GetRequiredService<DBService>();
            serialService.Messages.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
        {
            if (e?.NewItems != null)
            {
                foreach (SerialMessage item in e.NewItems)
                {
                    if (item.Type == MessageType.Ready)
                    {
                        var devices = _dbService.GetStoredDevices().Where(x => x.IsActivated).Select(x => new ZigbeeObject(x.ChannelNumber, false)).ToArray();
                        serialService.SendInitCommand(devices);
                    }
                    if (item.Type == MessageType.StateChanged)
                    {
                        if (item.Data != null)
                        {
                            var data = item.Data;
                            var channel = (int)data.channel.Value;
                            var state = (bool)data.state.Value;
                            var deviceId = _dbService.GetDeviceIdByChannel(channel);
                            _haService.SetState(deviceId, state);
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            serialService.Messages.CollectionChanged -= OnCollectionChanged;
            Task.FromResult(DisposeAsync);
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            serialService.Messages.CollectionChanged -= OnCollectionChanged;
            await _dbService.DisposeAsync();
            await _haService.DisposeAsync();
            await _scope.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
