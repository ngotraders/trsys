using Microsoft.EntityFrameworkCore;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Infrastructure.ReadModel.Database
{
    public class TrsysReadModelContext : DbContext, ITrsysReadModelContext
    {
        public TrsysReadModelContext(DbContextOptions<TrsysReadModelContext> options) : base(options)
        {
        }

        public DbSet<UserDto> Users { get; set; }
        public DbSet<UserPasswordHashDto> UserPasswordHashes { get; set; }
        public DbSet<SecretKeyDto> SecretKeys { get; set; }
        public DbSet<OrderDto> Orders { get; set; }
        public DbSet<LogDto> Logs { get; set; }
    }
}