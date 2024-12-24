using Microsoft.EntityFrameworkCore;
using ZigbeeBridgeAddon.Data;
using ZigbeeBridgeAddon.Data.Entities;
using ZigbeeBridgeAddon.SerialClient.Models;

namespace ZigbeeBridgeAddon.Services
{
    public class DBService(DevicesStore devicesStore, HAService haService) : IAsyncDisposable
    {
        public IQueryable<Device> GetStoredDevices() => devicesStore.Devices;

        public IEnumerable<ZigbeeObject> GetDevicesForBridge() 
        {
            return [.. GetStoredDevices().OrderBy(x => x.ChannelNumber).Where(x => x.IsActivated).Select(x => new ZigbeeObject(x.ChannelNumber, haService.GetDeviceState(x.Id)))];
        }

        public string GetDeviceIdByChannel(int channelNumber)
        {
            return devicesStore.Devices.First(x => x.ChannelNumber == channelNumber).Id;
        }

        public async Task AddDevice(string entityId, string area, string name, int channelNumber)
        {
            var device = new Device(entityId, area, name, channelNumber);
            devicesStore.Devices.Add(device);
            await devicesStore.SaveChangesAsync();
        }

        public async Task RemoveDevice(string entityId)
        {
            var device = await devicesStore.Devices.FirstOrDefaultAsync(x => x.Id == entityId);
            if (device != null)
            {
                devicesStore.Devices.Remove(device);
                await devicesStore.SaveChangesAsync();
            }
        }

        public async Task UpdateActivationState(string entityId, bool state)
        {
            var device = await devicesStore.Devices.FirstOrDefaultAsync(x => x.Id == entityId);
            if (device != null)
            {
                device.SetActivationState(state);
                await devicesStore.SaveChangesAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await haService.DisposeAsync();
            await devicesStore.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
