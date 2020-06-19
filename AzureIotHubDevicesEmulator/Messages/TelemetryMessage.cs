using System;

namespace AzureIotHubDevicesEmulator.Messages
{
    public abstract class TelemetryMessage
    {
        public string TenantId { get; set; }
        public string Model { get; set; }
        public Guid ObjectId { get; set; }
        public string Timestamp { get; set; }
    }
}
