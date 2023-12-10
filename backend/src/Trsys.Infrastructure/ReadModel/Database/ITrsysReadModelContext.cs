using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Infrastructure.ReadModel.Database
{
    public interface ITrsysReadModelContext : IDisposable
    {
        DbSet<UserDto> Users { get; set; }
        DbSet<UserPasswordHashDto> UserPasswordHashes { get; set; }
        DbSet<SecretKeyDto> SecretKeys { get; set; }
        DbSet<OrderDto> Orders { get; set; }
        DbSet<LogDto> Logs { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
