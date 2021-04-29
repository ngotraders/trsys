using Microsoft.Extensions.DependencyInjection;
using Trsys.Web.Authentication;
using Trsys.Web.Infrastructure.Generic;
using Trsys.Web.Infrastructure.InMemory;
using Trsys.Web.Infrastructure.SQLite;
using Trsys.Web.Models.Orders;
using Trsys.Web.Models.SecretKeys;
using Trsys.Web.Models.Users;
using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryStores(this IServiceCollection services)
        {
            services.AddSingleton<IAuthenticationTicketStore, InMemoryAuthenticationTicketStore>();
            services.AddSingleton<ISecretKeyUsageStore, InMemorySecretKeyUsageStore>();
            services.AddSingleton<IOrdersTextStore, InMemoryOrdersTextStore>();
            return services;
        }

        public static IServiceCollection AddSQLiteRepositories(this IServiceCollection services)
        {
            services.AddSingleton<TrsysContextProcessor>();
            services.AddTransient<IUserRepository, SQLiteUserRepository>();
            services.AddTransient<IOrderRepository, SQLiteOrderRepository>();
            services.AddTransient<ISecretKeyRepository, SQLiteSecretKeyRepository>();
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<ISecretKeyRepository, SecretKeyRepository>();
            return services;
        }
    }
}
