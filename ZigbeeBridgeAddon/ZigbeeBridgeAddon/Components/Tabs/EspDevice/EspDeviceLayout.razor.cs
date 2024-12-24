using ZigbeeBridgeAddon.Services;

namespace ZigbeeBridgeAddon.Components.Tabs.EspDevice
{
    public partial class EspDeviceLayout(SerialClientService serialService)
    {
        protected override void OnInitialized()
        {
        }

        private void UpdateDevice()
        {

        }
        private void RebootDevice()
        {
            serialService.SendRebootCommand();
        }
    }
}
