using Microsoft.EntityFrameworkCore;
using ZigbeeBridgeAddon.Data.Entities;

namespace ZigbeeBridgeAddon.Data
{
    public class DevicesStore : DbContext
    {
        public DevicesStore(DbContextOptions<DevicesStore> options) : base(options)
        {
        }
        public DbSet<Device> Devices { get; set; }
    }
}
