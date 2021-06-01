using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Trsys.Web.Infrastructure.InMemory;
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

            // Database
            services.AddSingleton<OrderInMemoryDatabase>();
            services.AddSingleton<SecretKeyInMemoryDatabase>();
            services.AddSingleton<UserInMemoryDatabase>();

            // Token management
            services.AddSingleton<TokenConnectionManager>();

            return services;
        }

        public static IServiceCollection AddInMemoryInfrastructure(this IServiceCollection services)
        {
            return services.AddInfrastructure().AddSingleton<IEventStore, InMemoryEventStore>();
        }
    }
}
