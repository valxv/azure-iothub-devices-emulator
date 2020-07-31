using System;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;

namespace AzureIotHubDevicesEmulator.Messages
{
    public class TimeSeriesMessageBuilder : IMessageBuilder
    {
        private static readonly Random _random = new Random();

        public Message BuildMessage(string tenantId, string deviceId, Guid objectId)
        {
            return new Message(
                JsonSerializer.SerializeToUtf8Bytes(
                    new TimeSeriesMessage
                    {
                        MessageType = "timeseries",
                        TenantId = tenantId,
                        ObjectId = objectId,
                        Model = "abb.ability.device",
                        Timestamp = DateTime.UtcNow.ToString("o"),
                        Variable = "temperature",
                        Value = Math.Round(_random.NextDouble() * 10 + 20, 1)
                    },
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
        }
    }
}
