using Radzen;
using Radzen.Blazor;
using ZigbeeBridgeAddon.Data.Entities;
using ZigbeeBridgeAddon.Services;

namespace ZigbeeBridgeAddon.Components.Tabs.Devices
{
    public partial class DevicesTabLayout(DBService dbService, DialogService dialogService)
    {
        private RadzenDataGrid<Device> grid { get; set; } = null!;
        private IEnumerable<Device> Devices { get; set; } = [];
        private bool SendButtonDisabled { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            Devices = dbService.GetStoredDevices().OrderBy(x => x.ChannelNumber);
        }

        private async Task ShowAddDeviceDialog()
        {
            var result = await dialogService.OpenAsync<AddDeviceDialogLayout>("Add device");
            dialogService.OnClose += async (object o) =>
            {
                if (o != null)
                {
                    await grid.RefreshDataAsync();
                    SendButtonDisabled = false;
                }
            };
        }

        private async Task OnStateChange(string deviceId, bool state)
        {
            await dbService.UpdateActivationState(deviceId, state);
            await grid.RefreshDataAsync();
            SendButtonDisabled = false;
        }

        private async Task OnRemove(string deviceId)
        {
            await dbService.RemoveDevice(deviceId);
            await grid.RefreshDataAsync();
            SendButtonDisabled = false;
        }
    }
}
