using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Configurations;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<TrsysContext>())
            {
                int retryCount = 0;
                while (retryCount < 10)
                {
                    try
                    {
                        if (db.Database.IsSqlServer())
                        {
                            await db.Database.MigrateAsync();
                        }
                        else
                        {
                            await db.Database.EnsureCreatedAsync();
                        }
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                        retryCount++;
                    }
                }
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
                await mediator.Send(new CreateUserIfNotExistsCommand("管理者", "admin", passwordHasher.Hash("P@ssw0rd"), "Administrator"));
            }
        }
    }
}
