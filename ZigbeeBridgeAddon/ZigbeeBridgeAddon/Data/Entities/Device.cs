namespace ZigbeeBridgeAddon.Data.Entities
{
    public class Device
    {
#pragma warning disable CS8618
        protected Device() { }
#pragma warning restore CS8618 

        public Device(string id, string area, string name, int channelNumber, bool isActivated = true) 
        {
            Id = id;
            Area = area;
            Name = name;
            ChannelNumber = channelNumber;
            IsActivated = isActivated;
        }

        public string Id { get; protected set; }
        public string Area { get; protected set; }
        public string Name { get; protected set; }
        public int ChannelNumber { get; protected set; }
        public bool IsActivated { get; protected set; }

        public void SetActivationState(bool activationState)
        {
            IsActivated = activationState;
        }
    }
}
