using Newtonsoft.Json;

namespace ZigbeeBridgeAddon.SerialClient.Models
{
    public class SerialSettings
    {
        [JsonProperty("ZB_ESP_DEVICE_PORT")]
        public string Port { get; set; }
    }
}
