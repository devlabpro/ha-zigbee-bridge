using Newtonsoft.Json;
using ZigbeeBridgeAddon.SerialClient.Enums;

namespace ZigbeeBridgeAddon.SerialClient.Models.Commands
{
    public class BaseCommand(MessageType type)
    {
        [JsonProperty("type")]
        public MessageType Type { get; protected set; } = type;
    }
}
