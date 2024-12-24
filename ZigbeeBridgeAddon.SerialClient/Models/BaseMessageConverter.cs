using Newtonsoft.Json.Converters;

namespace ZigbeeBridgeAddon.SerialClient.Models
{
    public class BaseMessageConverter : CustomCreationConverter<BaseMessage>
    {
        public override BaseMessage Create(Type objectType)
        {
            return new SerialMessage();
        }
    }
}
