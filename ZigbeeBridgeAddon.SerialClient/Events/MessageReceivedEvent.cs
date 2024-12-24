namespace ZigbeeBridgeAddon.SerialClient.Events
{
    public class MessageReceivedEvent(byte[] data)
    {
        public readonly byte[] Data = data;
    }
}
