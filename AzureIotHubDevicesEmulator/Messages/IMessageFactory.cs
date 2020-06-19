using System;
using Microsoft.Azure.Devices.Client;

namespace AzureIotHubDevicesEmulator.Messages
{
    public interface IMessageFactory
    {
        Message CreateMessage(TenantConfiguration tenantConfiguration, string deviceId, Guid objectId, Random random);
    }
}
