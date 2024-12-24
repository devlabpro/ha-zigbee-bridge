using Newtonsoft.Json;
using ZigbeeBridgeAddon.SerialClient.Enums;

namespace ZigbeeBridgeAddon.SerialClient.Models
{
    public class BaseMessage
    {
        [JsonProperty("type")]
        public MessageType Type { get; set; }
        [JsonProperty("data")]
        public dynamic? Data { get; set; }
    }
}
