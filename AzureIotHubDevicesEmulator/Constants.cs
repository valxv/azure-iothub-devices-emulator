namespace AzureIotHubDevicesEmulator.Constants
{
    public static class AbilityMessageHeaders
    {
        public const string MessageType = "ability-messagetype";
        public const string TenantId = "tenantId";
        public const string AbilityId = "ability-id";
    }

    public static class MessageTypes
    {
        public const string Alarm = "alarm";
        public const string Event = "event";
        public const string TimeSeries = "timeSeries";
    }
}
