using System.Text.Json;

namespace AzureIotHubDevicesEmulator.Messages
{
    public class AlarmMessage : TelemetryMessage
    {
        public string Alarm { get; set; }
        public int Value { get; set; }
    }
}
