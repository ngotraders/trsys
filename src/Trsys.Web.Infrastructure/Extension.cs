using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using System.Reflection;
using Trsys.Web.Infrastructure.SqlStreamStore;
using Trsys.Web.Infrastructure.Tokens;
using Trsys.Web.Models.ReadModel.Infrastructure;

namespace Trsys.Web.Infrastructure
{
    public static class Extension
    {
        private static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // MediatR dependencies
            services.AddMediatR(Assembly.Load("Trsys.Web.Models"), Assembly.Load("Trsys.Web.Infrastructure"));

            // Cqrs services without IEventStore
            services.AddSingleton<ICache, MemoryCache>();
            services.AddScoped<IRepository>(y => new CacheRepository(new Repository(y.GetService<IEventStore>()), y.GetService<IEventStore>(), y.GetService<ICache>()));
            services.AddScoped<ISession, Session>();

            // Event store
            services.AddSingleton<IEventStore, SqlStreamStoreEventStore>();

            // Database
            services.AddSingleton<OrderInMemoryDatabase>();
            services.AddSingleton<SecretKeyInMemoryDatabase>();
            services.AddSingleton<UserInMemoryDatabase>();
            services.AddSingleton<LogInMemoryDatabase>();

            // Token management
            services.AddSingleton<TokenConnectionManager>();

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
                services.AddSingleton<IMessageBus, InMemoryMessageBus>();
            }
            else
            {
                services.AddSingleton<IMessageBus, RedisMessageBus>();
                services.AddSingleton(new RedisMessageOptions()
                {
                    Configuration = redisConfiguration,
                });
            }

            return services;
        }
    }
}
