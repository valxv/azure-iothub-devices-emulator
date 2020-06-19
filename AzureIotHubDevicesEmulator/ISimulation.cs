using System;
using System.Threading.Tasks;

namespace AzureIotHubDevicesEmulator
{
    public interface ISimulation : IDisposable
    {
        Task Run();
    }
}
