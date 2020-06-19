using System;
using Microsoft.Azure.Devices.Client;

namespace AzureIotHubDevicesEmulator.Messages
{
    public interface IMessageBuilder
    {
        Message BuildMessage(string tenantId, string deviceId, Guid objectId);
    }
}
