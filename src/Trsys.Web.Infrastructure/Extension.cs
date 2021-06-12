using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using StackExchange.Redis;
using System.Reflection;
using Trsys.Web.Infrastructure.Messaging;
using Trsys.Web.Infrastructure.ReadModel.InMemory;
using Trsys.Web.Infrastructure.WriteModel.SqlStreamStore;
using Trsys.Web.Infrastructure.WriteModel.SqlStreamStore.InMemory;
using Trsys.Web.Infrastructure.WriteModel.SqlStreamStore.Redis;
using Trsys.Web.Infrastructure.WriteModel.Tokens;
using Trsys.Web.Infrastructure.WriteModel.Tokens.InMemory;
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
            return services.AddSqlServerInfrastructure(null, null);
        }

        public static IServiceCollection AddSqlServerInfrastructure(this IServiceCollection services, string connectionString, string redisConfiguration)
        {
            services.AddInfrastructure();

            if (string.IsNullOrEmpty(connectionString))
            {
                services.AddSingleton<IStreamStore, InMemoryStreamStore>();
            }
            else
            {
                services.AddTransient<IStreamStore, MsSqlStreamStoreV3>();
                services.AddSingleton(new MsSqlStreamStoreV3Settings(connectionString));
            }

            if (string.IsNullOrEmpty(redisConfiguration))
            {
                // Manage latest version for each stream in StreamStore
                services.AddSingleton<ILatestStreamVersionHolder, InMemoryLatestStreamVersionHolder>();
            }
            else
            {
                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfiguration));

                // Manage latest version for each stream in StreamStore
                services.AddSingleton<ILatestStreamVersionHolder, RedisLatestStreamVersionHolder>();
            }

            // Database
            services.AddSingleton<ILogDatabase, InMemoryLogDatabase>();
            services.AddSingleton<IOrderDatabase, InMemoryOrderDatabase>();
            services.AddSingleton<ISecretKeyDatabase, InMemorySecretKeyDatabase>();
            services.AddSingleton<IUserDatabase, InMemoryUserDatabase>();

            services.AddSingleton<ISecretKeyConnectionStore, InMemorySecretKeyConnectionStore>();

            return services;
        }
    }
}
