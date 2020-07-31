using System;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;

namespace AzureIotHubDevicesEmulator.Messages
{
    public class EventMessageBuilder : IMessageBuilder
    {
        private static readonly Random _random = new Random();

        public Message BuildMessage(string tenantId, string deviceId, Guid objectId)
        {
            return new Message(
                JsonSerializer.SerializeToUtf8Bytes(
                    new EventMessage
                    {
                        MessageType = "event",
                        TenantId = tenantId,
                        ObjectId = objectId,
                        Model = "abb.ability.device",
                        Timestamp = DateTime.UtcNow.ToString("o"),
                        Event = "test_event",
                        Value = new EventValue
                        {
                            EventCode = "171900",
                            Value = $"sensor_{(_random.NextDouble() > 0.5 ? "connected" : "disconnected")}"
                        }
                    },
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
        }
    }
}
