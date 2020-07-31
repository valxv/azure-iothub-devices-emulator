using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureIotHubDevicesEmulator.Messages;
using AzureIotHubDevicesEmulator.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureIotHubDevicesEmulator
{
    public class IotHubDevicesSimulator : IIotHubDevicesSimulator
    {
        private readonly ILogger<IotHubDevicesSimulator> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMessageFactory _messageFactory;
        private readonly IDictionary<TenantConfiguration, IList<DeviceInfo>> _tenantDeviceClients;

        private bool _disposedValue;

        private ObjectIdsPool _objectIdsPool;

        public IotHubDevicesSimulator(ILogger<IotHubDevicesSimulator> logger, IConfiguration configuration, IMessageFactory messageFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _messageFactory = messageFactory;
            _tenantDeviceClients = new Dictionary<TenantConfiguration, IList<DeviceInfo>>();
        }

        public async Task StartSendingMessagesAsync(CancellationToken cancellationToken)
        {
            await Initialize().ConfigureAwait(false);

            _logger.LogInformation("Start sending telemetry messages...");

            List<Task> runningTasks = new List<Task>();
            foreach (var tenant in _tenantDeviceClients)
            {
                _logger.LogInformation("Start sending messages for tenant {0}", tenant.Key.Id);
                foreach (var deviceInfo in tenant.Value)
                {
                    runningTasks.Add(
                        Task.Factory.StartNew(
                            () => StartSendingMessagesForDeviceAsync(tenant.Key, deviceInfo, cancellationToken),
                            TaskCreationOptions.LongRunning)
                        );
                }
            }

            await Task.WhenAll(runningTasks).ConfigureAwait(false);
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
                    foreach (var tenant in _tenantDeviceClients)
                    {
                        foreach (var deviceInfo in tenant.Value)
                        {
                            deviceInfo.DeviceClient.Dispose();
                        }
                    }
                }

                _disposedValue = true;
            }
        }

        private Task Initialize()
        {
            InitializeDeviceClients();
            return GetOrSetObjectIdsPoolAsync();
        }

        private void InitializeDeviceClients()
        {
            _logger.LogInformation("Initializing device clients...");

            var tenantConfiguration = new List<TenantConfiguration>();
            _configuration.Bind("SimulatedDevices:Tenants", tenantConfiguration);

            foreach (var tenant in tenantConfiguration)
            {
                var deviceClients = new List<DeviceInfo>();
                for (int i = 0; i < tenant.NumberOfDevices; i++)
                {
                    deviceClients.Add(new DeviceInfo
                    {
                        DeviceId = TenantConfiguration.BuildDeviceName(tenant, i),
                        DeviceClient = DeviceClient.CreateFromConnectionString(
                            _configuration["IotHubConnectionString"],
                            TenantConfiguration.BuildDeviceName(tenant, i))
                    });
                }
                _tenantDeviceClients.Add(tenant, deviceClients);
            }

            _logger.LogInformation($"Registered {_tenantDeviceClients.Sum(t => t.Value.Count)} device clients");
        }

        // Get or Set objectId pool for the tenant devices
        private async Task GetOrSetObjectIdsPoolAsync()
        {
            _logger.LogInformation("Setting up devices objectId pool...");

            var poolFilePath = _configuration["ObjectIdsPoolFilePath"];

            if (await TryLoadExistingPoolFromFileAsync(poolFilePath).ConfigureAwait(false))
                return;

            _logger.LogInformation("ObjectId pool doesn't exist. Creating a new one...");

            _objectIdsPool = new ObjectIdsPool { Tenants = new List<Tenant>() };

            foreach (var tenant in _tenantDeviceClients)
            {
                var tnt = new Tenant
                {
                    Id = tenant.Key.Id,
                    Devices = new List<TenantDevice>()
                };

                foreach (var deviceClient in tenant.Value)
                {
                    tnt.Devices.Add(new TenantDevice
                    {
                        DeviceId = deviceClient.DeviceId,
                        ObjectId = Guid.NewGuid()
                    });
                }

                _objectIdsPool.Tenants.Add(tnt);
            }

            using var writeStream = new FileStream(poolFilePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(writeStream, _objectIdsPool).ConfigureAwait(false);

            _logger.LogInformation("Device objectId pool successfully set up.");
        }

        private async Task<bool> TryLoadExistingPoolFromFileAsync(string poolFilePath)
        {
            if (!File.Exists(poolFilePath))
                return false;

            _logger.LogInformation("ObjectId pool exists. Reading...");

            using var readStream = new FileStream(poolFilePath, FileMode.Open);
            _objectIdsPool = await JsonSerializer.DeserializeAsync<ObjectIdsPool>(readStream).ConfigureAwait(false);

            if (_tenantDeviceClients.Count != _objectIdsPool.Tenants.Count)
            {
                _objectIdsPool = null;
                return false;
            }

            foreach (var tenant in _tenantDeviceClients)
            {
                var poolTenant = _objectIdsPool.Tenants.FirstOrDefault(t => t.Id == tenant.Key.Id);
                if (poolTenant == null)
                {
                    _objectIdsPool = null;
                    return false;
                }

                if (poolTenant.Devices.Count != tenant.Value.Count)
                {
                    _objectIdsPool = null;
                    return false;
                }
            }

            return true;
        }

        private async Task StartSendingMessagesForDeviceAsync(TenantConfiguration tenantConfiguration,
            DeviceInfo deviceInfo, CancellationToken cancellationToken)
        {
            var random = new Random();

            while (!cancellationToken.IsCancellationRequested)
            {
                var objectId = _objectIdsPool
                    .Tenants.First(t => t.Id == tenantConfiguration.Id)
                    .Devices.First(d => d.DeviceId == deviceInfo.DeviceId)
                    .ObjectId;

                var message = _messageFactory.CreateMessage(tenantConfiguration, deviceInfo.DeviceId, objectId, random);

                try
                {
                    await deviceInfo.DeviceClient.SendEventAsync(message, cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation($"Message for Tenant \"{tenantConfiguration.Id}\" DeviceId \"{deviceInfo.DeviceId}\" ObjectId \"{objectId}\" sent.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Device: {deviceInfo.DeviceId}; Error sending device message: {ex.Message}");
                }

                await Task.Delay(tenantConfiguration.SendingInterval, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
