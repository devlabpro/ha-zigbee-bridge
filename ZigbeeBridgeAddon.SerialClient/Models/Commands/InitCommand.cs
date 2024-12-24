using Newtonsoft.Json;
using ZigbeeBridgeAddon.SerialClient.Enums;

namespace ZigbeeBridgeAddon.SerialClient.Models.Commands
{
    public class InitCommand(MessageType type, IEnumerable<ZigbeeObject> data) : BaseCommand(type)
    {
        [JsonProperty("data")]
        public IEnumerable<ZigbeeObject> Data { get; protected set; } = data;
    }
}
