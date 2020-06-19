namespace AzureIotHubDevicesEmulator.Messages
{
    public class TimeSeriesMessage : TelemetryMessage
    {
        public string Variable { get; set; }
        public double Value { get; set; }
    }
}
