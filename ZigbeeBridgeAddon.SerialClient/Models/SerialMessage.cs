using Newtonsoft.Json;
using ZigbeeBridgeAddon.SerialClient.Enums;

namespace ZigbeeBridgeAddon.SerialClient.Models
{
    public class SerialMessage : BaseMessage
    {
        public SerialMessage() { }
        public SerialMessage(MessageType type, dynamic? data)
        {
            Type = type;
            Data = data;
        }

        [JsonIgnore]
        public DateTime Received => DateTime.Now;
    }
}
