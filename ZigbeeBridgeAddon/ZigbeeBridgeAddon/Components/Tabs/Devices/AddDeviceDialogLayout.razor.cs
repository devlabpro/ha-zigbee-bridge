using NetDaemon.HassModel.Entities;
using Radzen;
using ZigbeeBridgeAddon.Components.Tabs.Devices.Models;
using ZigbeeBridgeAddon.Services;

namespace ZigbeeBridgeAddon.Components.Tabs.Devices
{
    public partial class AddDeviceDialogLayout(HAService service, DBService dbService, DialogService dialogService)
    {
        private AddDeviceFormModel Model { get; set; } = new AddDeviceFormModel();
        private IEnumerable<Entity> HADevices { get; set; } = [];
        private IEnumerable<Data.Entities.Device> AddonDevices => [.. dbService.GetStoredDevices()];
        private bool Disabled => Model.Channel == null || string.IsNullOrEmpty(Model.DeviceId);
        private string? SearchText { get; set; }

        protected override void OnInitialized()
        {
            
        }

        private async Task OnLoadDevices(LoadDataArgs args)
        {
            var query = await Task.FromResult(service.GetDevices().Where(x => !AddonDevices.Select(d => d.Id).Contains(x.EntityId)));

            if (!string.IsNullOrEmpty(args.Filter))
            {
                query = query.Where(x => x.EntityId.ToUpper().Contains(args.Filter.ToUpper()) || (!string.IsNullOrEmpty(x.Registration?.Device?.Name) && x.Registration.Device.Name.ToUpper().Contains(args.Filter.ToUpper())));
            }

            HADevices = query.ToList();

            await InvokeAsync(StateHasChanged);
        }

        private async Task OnSubmit(AddDeviceFormModel model)
        {
            if (model.Channel != null && !string.IsNullOrEmpty(model.DeviceId))
            {
                var device = HADevices.First(x => x.EntityId == model.DeviceId);
                await dbService.AddDevice(model.DeviceId, device.Area ?? string.Empty, device.Registration?.Device?.Name ?? string.Empty, model.Channel.Value);
            }
            dialogService.Close(true);
        }

        private bool ValidateChannel()
        {
            return !AddonDevices.Any(e => e.ChannelNumber == Model?.Channel);
        }
    }
}
