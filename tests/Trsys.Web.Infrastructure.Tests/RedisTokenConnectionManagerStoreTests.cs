using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using Trsys.Web.Infrastructure.WriteModel.Tokens;

namespace Trsys.Web.Infrastructure.Tests
{
    [TestClass]
    [Ignore]
    public class RedisTokenConnectionManagerStoreTests
    {
        [TestMethod]
        public async Task When_add_token_Then_token_in_use_returns_false()
        {
            var connection = await ConnectionMultiplexer.ConnectAsync("127.0.0.1");
            var store = new List<INotification>();
            using var services = new ServiceCollection()
                .AddSingleton<List<INotification>>(store)
                .AddSingleton<IConnectionMultiplexer>(connection)
                .AddMediatR(typeof(NotificationHandler))
                .AddLogging()
                .BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var sut = new TokenConnectionManager(services.GetRequiredService<IServiceScopeFactory>());
            var id = Guid.NewGuid();
            await sut.AddAsync("Token", id);
            Assert.IsFalse(await sut.IsTokenInUseAsync("Token"));
        }

        class NotificationHandler : INotificationHandler<INotification>
        {
            public List<INotification> Notifications { get; }

            public NotificationHandler(List<INotification> notifications)
            {
                this.Notifications = notifications;
            }
            public Task Handle(INotification notification, CancellationToken cancellationToken)
            {
                Notifications.Add(notification);
                return Task.CompletedTask;
            }
        }


    }
}
