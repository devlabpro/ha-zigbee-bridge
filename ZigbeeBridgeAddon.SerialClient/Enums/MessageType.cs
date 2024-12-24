namespace ZigbeeBridgeAddon.SerialClient.Enums
{
    public enum MessageType
    {
        Ready = 0,
        Init = 1,
        StateUpdate = 2,
        StateChanged = 3,
        Log = 98,
        Reboot = 99
    }
}
