using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.Messaging;
using Trsys.Models.WriteModel.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Models
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<TrsysContext>();
            var store = scope.ServiceProvider.GetRequiredService<IStreamStore>();
            var tokenConnectionManager = scope.ServiceProvider.GetRequiredService<ISecretKeyConnectionManager>();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IMessageDispatcher>();
            await InitializeContextAsync(db);
            if (store is MsSqlStreamStoreV3 mssqlstore)
            {
                await mssqlstore.CreateSchemaIfNotExists();
            }

            var nextPosition = await InitializeReadModelAsync(store, 0, dispatcher);
            await InitializeWriteModelAsync(tokenConnectionManager);
            scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
            await InitializeReadModelAsync(store, nextPosition, dispatcher);
        }

        public static async Task InitializeContextAsync(TrsysContext db)
        {
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
        }

        public static async Task SeedDataAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var mediator = scope.ServiceProvider.GetService<IMediator>();
            var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>();
            var user = await userManager.FindByNameAsync("admin");
            var identity = new IdentityUser("admin");
            var passwordHash = userManager.PasswordHasher.HashPassword(identity, "P@ssw0rd");
            if (user != null)
            {
                await mediator.Send(new UserChangePasswordHashCommand(Guid.Parse(user.Id), passwordHash));
                return;
            }
            await mediator.Send(new UserCreateCommand("管理者", "admin", "admin@example.com", passwordHash, "Administrator"));
        }

        private static async Task InitializeWriteModelAsync(ISecretKeyConnectionManager tokenManager)
        {
            await tokenManager.InitializeAsync();
        }

        private static async Task<long> InitializeReadModelAsync(IStreamStore store, long position, IMessageDispatcher dispatcher)
        {
            var page = await store.ReadAllForwards(position, 1000, true);
            long nextPosition;
            while (true)
            {
                nextPosition = page.NextPosition;
                foreach (var message in page.Messages)
                {
                    await dispatcher.DispatchAsync(new PublishingMessage()
                    {
                        Id = message.MessageId,
                        Type = message.Type,
                        Data = await message.GetJsonData()
                    });
                }
                if (page.IsEnd)
                {
                    break;
                }
                page = await page.ReadNext();
            }
            return nextPosition;
        }
    }
}
