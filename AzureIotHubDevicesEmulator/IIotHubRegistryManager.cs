using System;
using System.Threading.Tasks;

namespace AzureIotHubDevicesEmulator
{
    public interface IIotHubRegistryManager : IDisposable
    {
        bool DeleteExistingIotHbDevicesBeforeRegistration { get; }

        Task RegisterDevicesAsync();

        Task UnregisterDevicesAsync();
    }
}
