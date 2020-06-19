namespace AzureIotHubDevicesEmulator
{
    public class TenantConfiguration
    {
        public string Id { get; set; }
        public int NumberOfDevices { get; set; }
        public string DevicePrefix { get; set; }
        public int SendingInterval { get; set; }
        public MessageDistribution MessageTypesDistributionPerDevice { get; set; }

        public static string BuildDeviceName(TenantConfiguration tenant, int deviceNumber)
        {
            return $"{tenant.DevicePrefix}{deviceNumber + 1}";
        }
    }

    public class MessageDistribution
    {
        public double TimeSeries { get; set; }
        public double Event { get; set; }
        public double Alarm { get; set; }
    }
}
