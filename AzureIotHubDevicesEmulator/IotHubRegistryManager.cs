using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureIotHubDevicesEmulator
{
    public class IotHubRegistryManager : IIotHubRegistryManager
    {
        private readonly ILogger<IotHubRegistryManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly RegistryManager _registryManager;
        private readonly IList<TenantConfiguration> _tenantConfigurations;
        private bool _disposedValue;

        public bool DeleteExistingIotHbDevicesBeforeRegistration { get; }

        public IotHubRegistryManager(ILogger<IotHubRegistryManager> logger, IConfiguration configuration)
        {
            _logger = logger;
            _registryManager = RegistryManager.CreateFromConnectionString(configuration["IotHubConnectionString"]);
            DeleteExistingIotHbDevicesBeforeRegistration = bool.Parse(configuration["DeleteExistingIotHbDevicesBeforeRegistration"]);
            var tenantConfig = new List<TenantConfiguration>();
            configuration.Bind("SimulatedDevices:Tenants", tenantConfig);
            _tenantConfigurations = tenantConfig;
            _configuration = configuration;
        }

        public async Task RegisterDevicesAsync()
        {
            _logger.LogInformation("Preparing device registrations...");
            Task[] tasks = new Task[_tenantConfigurations.Sum(t => t.NumberOfDevices)];
            int i = 0;
            foreach (var tenant in _tenantConfigurations)
            {
                for (int j = 0; j < tenant.NumberOfDevices; j++)
                {
                    tasks[i++] = RegisterDeviceAsync(TenantConfiguration.BuildDeviceName(tenant, j));
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            _logger.LogInformation($"Registered {tasks.Length} devices");
        }

        public async Task UnregisterDevicesAsync()
        {
            _logger.LogInformation("Unregistering existing devices...");

            var query = _registryManager.CreateQuery("select deviceId, etag from devices", 100);
            int count = 0;

            do
            {
                var json = (await query.GetNextAsJsonAsync()
                    .ConfigureAwait(false))
                    .Select(j => JsonSerializer.Deserialize<JsonElement>(j,
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            }));

                foreach (var deviceId in json.Select(j => j.GetProperty("deviceId").GetString()))
                {
                    await _registryManager.RemoveDeviceAsync(deviceId);
                    count++;
                }

                //await _registryManager.RemoveDevices2Async(json.Select(j =>
                //{
                //    count++;
                //    return new Device(j.GetProperty("deviceId").GetString())
                //    {
                //        ETag = j.GetProperty("etag").GetString()
                //    };
                //})).ConfigureAwait(false);
            } while (query.HasMoreResults);

            if (File.Exists(_configuration["ObjectIdsPoolFilePath"]))
            {
                File.Delete(_configuration["ObjectIdsPoolFilePath"]);
                _logger.LogInformation("ObjectIds pool deleted.");
            }

            _logger.LogInformation($"Unregistered {count} devices");
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _registryManager.Dispose();
                }

                _disposedValue = true;
            }
        }

        private async Task RegisterDeviceAsync(string deviceId)
        {
            Device device;
            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceId)).ConfigureAwait(false);
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(deviceId).ConfigureAwait(false);
            }
            _logger.LogInformation("Registered device {0} with symmetric key {1}", deviceId, device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
