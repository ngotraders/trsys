using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Configurations;
using Trsys.Web.Models.Users;
using Trsys.Web.Services;

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
                            db.Database.EnsureCreated();
                        }
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                        retryCount++;
                    }
                }
                if (!await db.Users.AnyAsync())
                {
                    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
                    db.Users.Add(new User()
                    {
                        Username = "admin",
                        Password = passwordHasher.Hash("P@ssw0rd"),
                        Role = "Administrator",
                    });
                    await db.SaveChangesAsync();
                }
                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                await orderService.RefreshOrderTextAsync();
            }

        }
    }
}
