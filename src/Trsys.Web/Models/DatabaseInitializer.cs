using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Configurations;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Models
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

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
            }

            using (var db = scope.ServiceProvider.GetRequiredService<IStreamStore>())
            {
                int retryCount = 0;
                bool success = false;
                Exception lastException = null;
                while (retryCount < 10)
                {
                    try
                    {
                        if (db is MsSqlStreamStoreV3 mssqlstore)
                        {
                            await mssqlstore.CreateSchemaIfNotExists();
                        }
                        success = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        lastException = e;
                        Thread.Sleep(1000);
                        retryCount++;
                    }
                }
                if (!success)
                {
                    throw new Exception("Failed to create SqlStreamStore schema.", lastException);
                }
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
                await mediator.Send(new CreateUserIfNotExistsCommand("管理者", "admin", passwordHasher.Hash("P@ssw0rd"), "Administrator"));
            }
        }
    }
}
