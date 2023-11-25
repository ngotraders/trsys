using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using StackExchange.Redis;
using System;
using System.Reflection;
using Trsys.Infrastructure.Logging;
using Trsys.Infrastructure.Messaging;
using Trsys.Infrastructure.ReadModel.InMemory;
using Trsys.Infrastructure.ReadModel.UserNotification;
using Trsys.Infrastructure.WriteModel;
using Trsys.Infrastructure.WriteModel.SqlStreamStore;
using Trsys.Infrastructure.WriteModel.SqlStreamStore.InMemory;
using Trsys.Infrastructure.WriteModel.SqlStreamStore.Redis;
using Trsys.Infrastructure.WriteModel.Tokens;
using Trsys.Infrastructure.WriteModel.Tokens.InMemory;
using Trsys.Infrastructure.WriteModel.Tokens.Redis;
using Trsys.Models.Messaging;
using Trsys.Models.ReadModel.Infrastructure;
using Trsys.Models.WriteModel.Infrastructure;

namespace Trsys.Infrastructure
{
    public static class Extension
    {
        public static IServiceCollection AddEmailSender(this IServiceCollection services, Action<EmailSenderConfiguration> action)
        {
            var config = new EmailSenderConfiguration();
            action?.Invoke(config);
            services.AddSingleton<IEmailSender>(new EmailSender(config));
            return services;
        }

        private static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // MediatR dependencies
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.Load("Trsys.Models"));
            });
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryPipelineBehavior<,>));

            // Cqrs services without IEventStore
            services.AddSingleton<ICache, MemoryCache>();
            services.AddScoped<IRepository>(y => new CacheRepository(new Repository(y.GetService<IEventStore>()), y.GetService<IEventStore>(), y.GetService<ICache>()));
            services.AddScoped<ISession, Session>();

            // Event store
            services.AddSingleton<IEventStore, SqlStreamStoreEventStore>();
            services.AddSingleton<IEventDatabase, SqlStreamEventDatabase>();

            // Token management
            services.AddSingleton<ISecretKeyConnectionManager, TokenConnectionManager>();

            // message dispatching
            services.AddSingleton<IMessageDispatcher, MessageDispatcher>();

            // User notification
            services.AddSingleton<IUserNotificationDispatcher, EmailMessageUserNotificationDispatcher>();

            return services;
        }

        public static IServiceCollection AddInMemoryInfrastructure(this IServiceCollection services)
        {
            // For testing
            services.AddLogging();
            return services.AddInfrastructure(null, null);
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string sqlserverConnection, string redisConnection)
        {
            services.AddInfrastructure();

            if (string.IsNullOrEmpty(redisConnection))
            {
                // Manage latest version for each stream in StreamStore
                services.AddSingleton<ILatestStreamVersionHolder, InMemoryLatestStreamVersionHolder>();
                services.AddSingleton<ISecretKeyConnectionManagerStore, InMemoryTokenConnectionManagerStore>();

                // Message synchronization
                services.AddSingleton<IMessagePublisher, LocalMessagePublisher>();
            }
            else
            {
                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

                // Manage latest version for each stream in StreamStore
                services.AddSingleton<ILatestStreamVersionHolder, RedisLatestStreamVersionHolder>();
                services.AddSingleton<ISecretKeyConnectionManagerStore, RedisTokenConnectionManagerStore>();

                // Message synchronization
                services.AddSingleton<IMessagePublisher, RedisMessageBroker>();
            }

            // ReadModel Database
            services.AddSingleton<IUserDatabase, InMemoryUserDatabase>();
            services.AddSingleton<ISecretKeyDatabase, InMemorySecretKeyDatabase>();
            services.AddSingleton<IOrderDatabase, InMemoryOrderDatabase>();
            services.AddSingleton<IOrderHistoryDatabase, InMemoryOrderHistoryDatabase>();
            services.AddSingleton<ILogDatabase, InMemoryLogDatabase>();

            if (string.IsNullOrEmpty(sqlserverConnection))
            {
                services.AddSingleton<IStreamStore, InMemoryStreamStore>();
            }
            else
            {
                services.AddTransient<IStreamStore, MsSqlStreamStoreV3>();
                services.AddSingleton(new MsSqlStreamStoreV3Settings(sqlserverConnection));
            }

            return services;
        }
    }
}
