using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Infrastructure;
using Trsys.Web.Models.WriteModel.Notifications;

namespace Trsys.Web.Models.Tests
{
    [TestClass]
    public class InvalidateSecretTokenCommandTests
    {
        [TestMethod]
        public async Task Given_token_is_same_as_generated_Then_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Send(new InvalidateSecretTokenCommand(id, token));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(5, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[3].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[3]).Token);
            Assert.AreEqual(typeof(SecretKeyTokenInvalidated), events[4].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenInvalidated)events[4]).Token);
        }

        [TestMethod]
        public async Task Given_token_is_not_same_as_generated_Then_fails()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await mediator.Send(new InvalidateSecretTokenCommand(id, "InvalidToken")));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[3].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[3]).Token);
        }

        [TestMethod]
        public async Task Given_token_is_connected_Then_disconnects_and_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Publish(new SecretKeyConnected(id));
            await mediator.Send(new InvalidateSecretTokenCommand(id, token));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(5, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[3].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[3]).Token);
            Assert.AreEqual(typeof(SecretKeyTokenInvalidated), events[4].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenInvalidated)events[4]).Token);

            var connectionStore = services.GetRequiredService<ISecretKeyConnectionManager>();
            Assert.IsFalse(await connectionStore.IsConnectedAsync(id));
        }
    }
}
