using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIotHubDevicesEmulator
{
    public interface IIotHubDevicesSimulator : IDisposable
    {
        Task StartSendingMessagesAsync(CancellationToken cancellationToken);
    }
}
