using System;
using System.Collections.Generic;

namespace AzureIotHubDevicesEmulator.Models
{
    public class ObjectIdsPool
    {
        public IList<Tenant> Tenants { get; set; }
    }

    public class Tenant
    {
        public string Id { get; set; }
        public IList<TenantDevice> Devices { get; set; }
    }

    public class TenantDevice
    {
        public string DeviceId { get; set; }
        public Guid ObjectId { get; set; }
    }
}
