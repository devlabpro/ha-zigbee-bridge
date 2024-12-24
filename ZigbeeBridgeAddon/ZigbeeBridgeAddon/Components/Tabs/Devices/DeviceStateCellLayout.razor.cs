using Microsoft.AspNetCore.Components;
using System.Reactive.Linq;
using ZigbeeBridgeAddon.Data.Entities;
using ZigbeeBridgeAddon.Services;

namespace ZigbeeBridgeAddon.Components.Tabs.Devices
{
    public partial class DeviceStateCellLayout(HAService haService) : IDisposable
    {
        [Parameter]
        public Device? Device { get; set; }
        public bool State { get; set; }
        private IDisposable? Subscription { get; set; }

        protected override void OnInitialized()
        {
            if (Device != null)
            {
                State = haService.GetDeviceState(Device.Id);
                Subscription = haService.GetDeviceStateChanged(Device.Id).Subscribe(async (x) =>
                {
                    State = x.New?.State == "on";
                    await InvokeAsync(StateHasChanged);
                });
            }
        }

        public void Dispose()
        {
            Subscription?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
