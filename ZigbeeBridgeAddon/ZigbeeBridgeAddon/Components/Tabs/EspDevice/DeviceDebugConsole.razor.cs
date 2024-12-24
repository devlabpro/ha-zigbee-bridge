using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;
using System.Collections.Specialized;
using ZigbeeBridgeAddon.SerialClient.Enums;
using ZigbeeBridgeAddon.SerialClient.Models;
using ZigbeeBridgeAddon.Services;

namespace ZigbeeBridgeAddon.Components.Tabs.EspDevice
{
    public class Message
    {
        public DateTime Date { get; set; }
        public string Text { get; set; } = null!;
        public AlertStyle AlertStyle { get; set; }
    }

    public partial class DeviceDebugConsole(SerialClientService serialService) : IDisposable
    {
        public IList<Message> messages = [];

        protected override void OnInitialized()
        {
            foreach(var msg in serialService.Messages)
            {
                AddNewLogMessage(msg);
            }
            serialService.Messages.CollectionChanged += OnCollectionChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                await JSRuntime.InvokeVoidAsync("eval", $"document.getElementById('log-console').scrollTop = document.getElementById('log-console').scrollHeight");
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
        {
            if (e?.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    AddNewLogMessage((SerialMessage)item);
                }
            }
        }

        private void AddNewLogMessage(SerialMessage msg)
        {
            string? text;
            if (msg.Type == MessageType.Log)
            {
                text = (string?)msg.Data;
            }
            else if (msg.Type == MessageType.Ready)
            {
                text = msg.Type.ToString();
            }
            else
            {
                text = JsonConvert.SerializeObject(msg);
            }
            if (!string.IsNullOrEmpty(text))
            {
                messages.Add(new Message { Date = msg.Received, Text = text, AlertStyle = AlertStyle.Info });
                InvokeAsync(StateHasChanged);
            }
        }

        void OnClearClick()
        {
            Clear();
        }

        public void Clear()
        {
            messages.Clear();
            serialService.ClearLog();
            InvokeAsync(StateHasChanged);
        }

        public void Log(string message, AlertStyle alertStyle = AlertStyle.Info)
        {
            messages.Add(new Message { Date = DateTime.Now, Text = message, AlertStyle = alertStyle });

            InvokeAsync(StateHasChanged);
        }

        public void Dispose() => serialService.Messages.CollectionChanged -= OnCollectionChanged;
    }
}
