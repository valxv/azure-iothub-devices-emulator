namespace AzureIotHubDevicesEmulator.Messages
{
    public class EventMessage : TelemetryMessage
    {
        public string Event { get; set; }
        public EventValue Value { get; set; }
    }

    public class EventValue
    {
        public string EventCode { get; set; }
        public string Value { get; set; }
    }
}
