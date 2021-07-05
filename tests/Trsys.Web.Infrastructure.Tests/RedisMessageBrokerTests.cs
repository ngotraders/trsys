using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Messaging;
using Trsys.Web.Models.Messaging;
using Trsys.Web.Models.WriteModel.Notifications;

namespace Trsys.Web.Infrastructure.Tests
{
    [TestClass]
    [Ignore]
    public class RedisMessageBrokerTests
    {
        [TestMethod]
        public async Task When_message_publish_Then_task_completes_and_published_through_MediatR()
        {
            var store = new List<SecretKeyConnected>();
            using var services = new ServiceCollection()
                .AddSingleton<List<SecretKeyConnected>>(store)
                .AddMediatR(typeof(TestHandler1))
                .AddSingleton<IMessageDispatcher, MessageDispatcher>()
                .AddLogging()
                .BuildServiceProvider();
            var connection = await ConnectionMultiplexer.ConnectAsync("127.0.0.1");
            var dispatcher = services.GetRequiredService<IMessageDispatcher>();
            var logger = services.GetRequiredService<ILogger<RedisMessageBroker>>();
            var sut = new RedisMessageBroker(connection, dispatcher, logger);
            await sut.Enqueue(PublishingMessageEnvelope.Create(new SecretKeyConnected(Guid.Empty)));
            Assert.AreEqual(Guid.Empty, store.First().Id);
        }

        [TestMethod]
        public async Task When_message_publish_and_mediatr_handler_throws_Then_publish_completes()
        {
            using var services = new ServiceCollection()
                .AddMediatR(typeof(TestHandler2))
                .AddSingleton<IMessageDispatcher, MessageDispatcher>()
                .AddLogging()
                .BuildServiceProvider();
            var connection = await ConnectionMultiplexer.ConnectAsync("127.0.0.1");
            var dispatcher = services.GetRequiredService<IMessageDispatcher>();
            var logger = services.GetRequiredService<ILogger<RedisMessageBroker>>();
            var sut = new RedisMessageBroker(connection, dispatcher, logger);
            await sut.Enqueue(PublishingMessageEnvelope.Create(new SecretKeyConnected(Guid.Empty)));
        }

        [TestMethod]
        public async Task Given_connection_is_not_active_Then_throws_error_on_enqueu()
        {
            var store = new List<SecretKeyConnected>();
            using var services = new ServiceCollection()
                .AddMediatR(typeof(TestHandler1))
                .AddSingleton<IMessageDispatcher, MessageDispatcher>()
                .AddLogging()
                .BuildServiceProvider();
            var connection = await ConnectionMultiplexer.ConnectAsync("unknown");
            var dispatcher = services.GetRequiredService<IMessageDispatcher>();
            var logger = services.GetRequiredService<ILogger<RedisMessageBroker>>();
            var sut = new RedisMessageBroker(connection, dispatcher, logger);
            await sut.Enqueue(PublishingMessageEnvelope.Create(new SecretKeyConnected(Guid.Empty)));
        }

        class TestHandler1 : INotificationHandler<SecretKeyConnected>
        {
            public List<SecretKeyConnected> Notifications { get; }

            public TestHandler1(List<SecretKeyConnected> notifications)
            {
                this.Notifications = notifications;
            }
            public Task Handle(SecretKeyConnected notification, CancellationToken cancellationToken)
            {
                Notifications.Add(notification);
                return Task.CompletedTask;
            }
        }

        class TestHandler2 : INotificationHandler<SecretKeyConnected>
        {
            public Task Handle(SecretKeyConnected notification, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("InvalidOperation");
            }
        }

    }
}
