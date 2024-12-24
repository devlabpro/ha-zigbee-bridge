using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel;

namespace ZigbeeBridgeAddon.Services
{
    public class HAService(IHaContext ha) : IAsyncDisposable
    {
        public bool Disposed { get; set; }

        public IReadOnlyList<Entity> GetDevices()
        {
            var result = ha.GetAllEntities().Where(x => x.EntityId.ToUpper().StartsWith("SWITCH") || x.EntityId.ToUpper().StartsWith("LIGHT")).OrderBy(x => x.EntityId);
            return [.. result];
        }

        public bool GetDeviceState(string entityId)
        {
            return ha.Entity(entityId).State == "on";
        }

        public IObservable<StateChange> GetDeviceStateChanged(string entityId)
        {
            return ha.Entity(entityId).StateChanges();
        }

        public void SetState(string entityId, bool state)
        {
            var cmd = state ? "turn_on" : "turn_off";
            ha.Entity(entityId).CallService(cmd);
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
        }
    }
}
