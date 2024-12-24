namespace ZigbeeBridgeAddon.SerialClient.Events
{
    public class ConnectionStatusChangedEvent(bool state)
    {
        public readonly bool Connected = state;
    }
}
