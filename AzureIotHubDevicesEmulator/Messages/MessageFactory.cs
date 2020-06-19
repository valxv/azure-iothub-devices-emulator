using System;
using System.Collections.Generic;
using System.Linq;
using AzureIotHubDevicesEmulator.Constants;
using Microsoft.Azure.Devices.Client;

namespace AzureIotHubDevicesEmulator.Messages
{
    public class MessageFactory : IMessageFactory
    {
        private readonly IDictionary<string, IMessageBuilder> _messageBuilders;

        public MessageFactory(IDictionary<string, IMessageBuilder> messageBuilders)
        {
            _messageBuilders = messageBuilders;
        }

        public Message CreateMessage(TenantConfiguration tenantConfiguration, string deviceId, Guid objectId, Random random)
        {
            // get telemetry message type according to the configured distribution
            var messageType = GetMessageType(random.NextDouble(), tenantConfiguration.MessageTypesDistributionPerDevice);

            return _messageBuilders[messageType].BuildMessage(tenantConfiguration.Id, deviceId, objectId);
        }

        private string GetMessageType(double randomValue, MessageDistribution messageDistribution)
        {
            Dictionary<double, string> distribution = new Dictionary<double, string>
            {
                { 0, MessageTypes.Alarm },
                { messageDistribution.Event, MessageTypes.Event },
                { messageDistribution.TimeSeries, MessageTypes.TimeSeries }
            };

            return distribution.OrderByDescending(kv => kv.Key).First(kv => kv.Key <= randomValue).Value;
        }
    }
}
