using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AzureIotHubDevicesEmulator
{
    public class Simulation : ISimulation
    {
        private readonly ILogger<Simulation> _logger;
        private readonly IIotHubRegistryManager _registryManager;
        private readonly IIotHubDevicesSimulator _devicesSimulator;
        private bool _disposedValue;

        public Simulation(ILogger<Simulation> logger, IIotHubRegistryManager registryManager, IIotHubDevicesSimulator devicesSimulator)
        {
            _logger = logger;
            _registryManager = registryManager;
            _devicesSimulator = devicesSimulator;
        }

        public async Task Run()
        {
            await RegisterSimulatedDevicesAsync().ConfigureAwait(false);
            await StartSendingTelemetryAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private async Task RegisterSimulatedDevicesAsync()
        {
            if (_registryManager.DeleteExistingIotHbDevicesBeforeRegistration)
            {
                await _registryManager.UnregisterDevicesAsync().ConfigureAwait(false);
            }

            await _registryManager.RegisterDevicesAsync().ConfigureAwait(false);
        }

        private async Task StartSendingTelemetryAsync()
        {
            var exitEvent = new ManualResetEvent(false);
            var ctSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) => {
                ctSource.Cancel();
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            var workerTask = Task.Run(() => _devicesSimulator.StartSendingMessagesAsync(ctSource.Token), ctSource.Token);

            exitEvent.WaitOne();

            _logger.LogInformation("Program cancellation requested! Finishing active threads...");

            await Task.WhenAny(workerTask, Task.Delay(5000)).ConfigureAwait(false);

            _logger.LogInformation("All working threads stopped. Exiting...");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _registryManager.Dispose();
                    _devicesSimulator.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
