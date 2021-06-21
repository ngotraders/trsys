using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using Trsys.Web.Infrastructure.Messaging;
using Trsys.Web.Models.Messaging;
using Trsys.Web.Models.WriteModel.Notifications;

namespace Trsys.Web.Infrastructure.Tests
{
    [TestClass]
    public class RedisMessageBrokerTests
    {
        [TestMethod]
        public async Task When_message_publish_Then_task_completes_and_published_through_MediatR()
        {
            var store = new List<TokenTouched>();
            using var services = new ServiceCollection()
                .AddSingleton<List<TokenTouched>>(store)
                .AddMediatR(typeof(TestHandler1))
                .AddLogging()
                .BuildServiceProvider();
            var connection = await ConnectionMultiplexer.ConnectAsync("127.0.0.1");
            var mediator = services.GetRequiredService<IMediator>();
            var logger = services.GetRequiredService<ILogger<RedisMessageBroker>>();
            var sut = new RedisMessageBroker(connection, mediator, logger);
            await sut.Enqueue(PublishingMessageEnvelope.Create(new TokenTouched("Token")));
            Assert.AreEqual("Token", store.First().Token);
        }

        [TestMethod]
        public async Task When_message_publish_and_mediatr_handler_throws_Then_publish_completes()
        {
            using var services = new ServiceCollection()
                .AddMediatR(typeof(TestHandler2))
                .AddLogging()
                .BuildServiceProvider();
            var connection = await ConnectionMultiplexer.ConnectAsync("127.0.0.1");
            var mediator = services.GetRequiredService<IMediator>();
            var logger = services.GetRequiredService<ILogger<RedisMessageBroker>>();
            var sut = new RedisMessageBroker(connection, mediator, logger);
            await sut.Enqueue(PublishingMessageEnvelope.Create(new TokenTouched("Token")));
        }

        [TestMethod]
        public async Task Given_connection_is_not_active_Then_throws_error_on_enqueu()
        {
            var store = new List<TokenTouched>();
            using var services = new ServiceCollection()
                .AddMediatR(typeof(TestHandler1))
                .AddLogging()
                .BuildServiceProvider();
            var connection = await ConnectionMultiplexer.ConnectAsync("unknown");
            var mediator = services.GetRequiredService<IMediator>();
            var logger = services.GetRequiredService<ILogger<RedisMessageBroker>>();
            var sut = new RedisMessageBroker(connection, mediator, logger);
            await sut.Enqueue(PublishingMessageEnvelope.Create(new TokenTouched("Token")));
        }

        class TestHandler1 : INotificationHandler<TokenTouched>
        {
            public List<TokenTouched> Notifications { get; }

            public TestHandler1(List<TokenTouched> notifications)
            {
                this.Notifications = notifications;
            }
            public Task Handle(TokenTouched notification, CancellationToken cancellationToken)
            {
                Notifications.Add(notification);
                return Task.CompletedTask;
            }
        }

        class TestHandler2 : INotificationHandler<TokenTouched>
        {
            public Task Handle(TokenTouched notification, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("InvalidOperation");
            }
        }

    }
}
