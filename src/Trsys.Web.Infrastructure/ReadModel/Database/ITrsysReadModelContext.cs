using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Infrastructure.ReadModel.Database
{
    public interface ITrsysReadModelContext : IDisposable
    {
        DbSet<UserDto> Users { get; set; }
        DbSet<SecretKeyDto> SecretKeys { get; set; }
        DbSet<OrderDto> Orders { get; set; }
        DbSet<LogDto> Logs { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
