using Microsoft.EntityFrameworkCore;
using Trsys.Web.Models;

namespace Trsys.Web.Data
{
    public class TrsysContext : DbContext
    {
        public TrsysContext(DbContextOptions<TrsysContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<SecretKey> SecretKeys { get; set; }
        public DbSet<User> Users { get; set; }
    }
}