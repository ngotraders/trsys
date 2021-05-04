using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using System;
using Trsys.Web.Authentication;
using Trsys.Web.Infrastructure.EventProcessing;
using Trsys.Web.Infrastructure.Generic;
using Trsys.Web.Infrastructure.InMemory;
using Trsys.Web.Infrastructure.Redis;
using Trsys.Web.Infrastructure.SQLite;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.Orders;
using Trsys.Web.Models.SecretKeys;
using Trsys.Web.Models.Users;
using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddEventProcessor(this IServiceCollection services)
        {
            services.AddSingleton<IEventSubmitter, EventSubmitter>();
            services.AddSingleton<EventQueue>();
            services.AddHostedService<EventQueueProcessor>();
            return services;
        }

        public static IServiceCollection AddInMemoryStores(this IServiceCollection services)
        {
            services.AddSingleton<IAuthenticationTicketStore, InMemoryAuthenticationTicketStore>();
            services.AddSingleton<ISecretKeyUsageStore, InMemorySecretKeyUsageStore>();
            services.AddSingleton<IOrdersTextStore, InMemoryOrdersTextStore>();
            return services;
        }

        public static IServiceCollection AddRedisStores(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
        {
            services.AddStackExchangeRedisCache(setupAction);
            services.AddSingleton<IAuthenticationTicketStore, RedisAuthenticationTicketStore>();
            services.AddSingleton<ISecretKeyUsageStore, RedisSecretKeyUsageStore>();
            services.AddSingleton<IOrdersTextStore, RedisOrdersTextStore>();
            return services;
        }

        public static IServiceCollection AddSQLiteRepositories(this IServiceCollection services)
        {
            services.AddSingleton<TrsysContextProcessor>();
            services.AddTransient<IUserRepository, SQLiteUserRepository>();
            services.AddTransient<IOrderRepository, SQLiteOrderRepository>();
            services.AddTransient<ISecretKeyRepository, SQLiteSecretKeyRepository>();
            services.AddTransient<IEventRepository, SQLiteEventRepository>();
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<ISecretKeyRepository, SecretKeyRepository>();
            services.AddTransient<IEventRepository, EventRepository>();
            return services;
        }
    }
}
