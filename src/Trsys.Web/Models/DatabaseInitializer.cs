using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Configurations;
using Trsys.Web.Infrastructure.SqlStreamStore;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Models
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<TrsysContext>();
            using var store = scope.ServiceProvider.GetRequiredService<IStreamStore>();
            int retryCount = 0;
            bool success = false;
            Exception lastException = null;
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
                    if (store is MsSqlStreamStoreV3 mssqlstore)
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
            var page = await store.ReadAllForwards(0, 100, true);
            while (true)
            {
                foreach (var message in page.Messages)
                {
                    await mediator.Publish(MessageConverter.ConvertToNotification(new PublishingMessage()
                    {
                        Id = message.MessageId,
                        Type = message.Type,
                        Data = await message.GetJsonData()
                    }));
                }
                if (page.IsEnd)
                {
                    break;
                }
                page = await page.ReadNext();
            }
        }

        public static async Task SeedDataAsync(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
                await mediator.Send(new CreateUserIfNotExistsCommand("管理者", "admin", passwordHasher.Hash("P@ssw0rd"), "Administrator"));
            }
        }
    }
}
