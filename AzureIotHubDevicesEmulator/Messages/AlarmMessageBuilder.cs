using System;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;

namespace AzureIotHubDevicesEmulator.Messages
{
    public class AlarmMessageBuilder : IMessageBuilder
    {
        public Message BuildMessage(string tenantId, string deviceId, Guid objectId)
        {
            return new Message(
                JsonSerializer.SerializeToUtf8Bytes(
                    new AlarmMessage
                    {
                        MessageType = "alarm",
                        TenantId = tenantId,
                        ObjectId = objectId,
                        Model = "abb.ability.device",
                        Timestamp = DateTime.UtcNow.ToString("o"),
                        Alarm = "test_alarm",
                        Value = 10
                    },
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
        }
    }
}
