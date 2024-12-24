using Newtonsoft.Json;

namespace ZigbeeBridgeAddon.SerialClient.Models
{
    public class ZigbeeObject
    {
        public ZigbeeObject() { }   
        public ZigbeeObject(int channel, bool state)
        {
            Channel = channel;
            State = state;
        }

        [JsonProperty("channel")]
        public int Channel { get; set; }

        [JsonProperty("state")]
        public bool State { get; set; }
    }
}
