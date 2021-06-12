using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using StackExchange.Redis;
using System.Reflection;
using Trsys.Web.Infrastructure.Messaging;
using Trsys.Web.Infrastructure.ReadModel.Database;
using Trsys.Web.Infrastructure.ReadModel.InMemory;
using Trsys.Web.Infrastructure.WriteModel.SqlStreamStore;
using Trsys.Web.Infrastructure.WriteModel.SqlStreamStore.InMemory;
using Trsys.Web.Infrastructure.WriteModel.SqlStreamStore.Redis;
using Trsys.Web.Infrastructure.WriteModel.Tokens;
using Trsys.Web.Infrastructure.WriteModel.Tokens.InMemory;
using Trsys.Web.Infrastructure.WriteModel.Tokens.Redis;
using Trsys.Web.Models.Messaging;
using Trsys.Web.Models.ReadModel.Infrastructure;
using Trsys.Web.Models.WriteModel.Infrastructure;

namespace Trsys.Web.Infrastructure
{
    public static class Extension
    {
        private static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // MediatR dependencies
            services.AddMediatR(Assembly.Load("Trsys.Web.Models"));

            // Cqrs services without IEventStore
            services.AddSingleton<ICache, MemoryCache>();
            services.AddScoped<IRepository>(y => new CacheRepository(new Repository(y.GetService<IEventStore>()), y.GetService<IEventStore>(), y.GetService<ICache>()));
            services.AddScoped<ISession, Session>();

            // Event store
            services.AddSingleton<IEventStore, SqlStreamStoreEventStore>();

            // Token management
            services.AddSingleton<ITokenConnectionManager, TokenConnectionManager>();

            // Message synchronization
            services.AddSingleton<IPublishingMessageProcessor, PublishingMessageProcessor>();

            return services;
        }

        public static IServiceCollection AddInMemoryInfrastructure(this IServiceCollection services)
        {
            return services.AddInfrastructure(null, null);
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string sqlserverConnection, string redisConnection)
        {
            services.AddInfrastructure();

            if (string.IsNullOrEmpty(redisConnection))
            {
                // Manage latest version for each stream in StreamStore
                services.AddSingleton<ILatestStreamVersionHolder, InMemoryLatestStreamVersionHolder>();
                services.AddSingleton<ISecretKeyConnectionStore, InMemorySecretKeyConnectionStore>();
            }
            else
            {
                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

                // Manage latest version for each stream in StreamStore
                services.AddSingleton<ILatestStreamVersionHolder, RedisLatestStreamVersionHolder>();
                services.AddSingleton<ISecretKeyConnectionStore, RedisSecretKeyConnectionStore>();
            }

            if (string.IsNullOrEmpty(sqlserverConnection))
            {
                services.AddSingleton<IStreamStore, InMemoryStreamStore>();

                // ReadModel Database
                services.AddSingleton<IUserDatabase, InMemoryUserDatabase>();
                services.AddSingleton<ISecretKeyDatabase, InMemorySecretKeyDatabase>();
                services.AddSingleton<IOrderDatabase, InMemoryOrderDatabase>();
                services.AddSingleton<ILogDatabase, InMemoryLogDatabase>();
            }
            else
            {
                services.AddTransient<IStreamStore, MsSqlStreamStoreV3>();
                services.AddSingleton(new MsSqlStreamStoreV3Settings(sqlserverConnection));

                // WriteModel Database
                services.AddTransient<IUserDatabase, SqlServerUserDatabase>();
                services.AddSingleton<ISecretKeyDatabase, InMemorySecretKeyDatabase>();
                services.AddTransient<IOrderDatabase, InMemoryOrderDatabase>();
                services.AddTransient<ILogDatabase, SqlServerLogDatabase>();
            }

            return services;
        }
    }
}
