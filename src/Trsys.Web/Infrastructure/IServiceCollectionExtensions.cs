using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using System;
using Trsys.Web.Infrastructure.Generic;
using Trsys.Web.Infrastructure.SQLite;
using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisStores(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
        {
            services.AddStackExchangeRedisCache(setupAction);
            return services;
        }

        public static IServiceCollection AddSQLiteRepositories(this IServiceCollection services)
        {
            services.AddSingleton<TrsysContextProcessor>();
            services.AddTransient<IEventRepository, SQLiteEventRepository>();
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IEventRepository, EventRepository>();
            return services;
        }
    }
}
