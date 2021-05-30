using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Trsys.Web.Infrastructure
{
    public static class Extension
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // MediatR dependencies
            services.AddMediatR(Assembly.Load("Trsys.Web.Models"));

            // Cqrs services
            services.AddSingleton<IEventStore, InMemoryEventStore>();
            services.AddSingleton<ICache, MemoryCache>();
            services.AddScoped<IRepository>(y => new CacheRepository(new Repository(y.GetService<IEventStore>()), y.GetService<IEventStore>(), y.GetService<ICache>()));
            services.AddScoped<ISession, Session>();

            return services;
        }
    }
}
