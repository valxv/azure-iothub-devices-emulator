using System.Text.Json;

namespace AzureIotHubDevicesEmulator.Messages
{
    public class TimeSeriesMessage : TelemetryMessage
    {
        public string Variable { get; set; }
        public dynamic Value { get; set; }
    }
}
