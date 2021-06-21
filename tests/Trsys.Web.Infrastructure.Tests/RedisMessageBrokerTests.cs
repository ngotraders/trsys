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
                .AddMediatR(typeof(TestHandler))
                .AddLogging()
                .BuildServiceProvider();
            var connection = await ConnectionMultiplexer.ConnectAsync("127.0.0.1");
            var mediator = services.GetRequiredService<IMediator>();
            var logger = services.GetRequiredService<ILogger<RedisMessageBroker>>();
            var sut = new RedisMessageBroker(connection, mediator, logger);
            await sut.Enqueue(PublishingMessageEnvelope.Create(new TokenTouched("Token")));
            Assert.AreEqual("Token", store.First().Token);
        }

        class TestHandler : INotificationHandler<TokenTouched>
        {
            public List<TokenTouched> Notifications { get; }

            public TestHandler(List<TokenTouched> notifications)
            {
                this.Notifications = notifications;
            }
            public Task Handle(TokenTouched notification, CancellationToken cancellationToken)
            {
                Notifications.Add(notification);
                return Task.CompletedTask;
            }
        }
    }
}
