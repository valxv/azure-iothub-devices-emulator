using Microsoft.Azure.Devices.Client;

namespace AzureIotHubDevicesEmulator.Models
{
    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public DeviceClient DeviceClient { get; set; }
    }
}
