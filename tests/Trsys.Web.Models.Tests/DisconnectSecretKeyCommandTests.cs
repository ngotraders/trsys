using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models.ReadModel.Events;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Infrastructure;

namespace Trsys.Web.Models.Tests
{
    [TestClass]
    public class DisconnectSecretKeyCommandTests
    {
        [TestMethod]
        public async Task When_disconnected_with_connected_token_then_successfully_disconnects()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Send(new ConnectSecretKeyCommand(id, token));
            await mediator.Send(new DisconnectSecretKeyCommand(id, token));

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

            var connectionStore = services.GetRequiredService<ISecretKeyConnectionStore>();
            Assert.IsFalse(await connectionStore.IsTokenInUseAsync(id));
        }

        [TestMethod]
        public async Task When_disconnect_with_invalid_token_then_not_happens()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Send(new ConnectSecretKeyCommand(id, token));
            await mediator.Send(new DisconnectSecretKeyCommand(id, "InvalidToken"));

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

            var connectionStore = services.GetRequiredService<ISecretKeyConnectionStore>();
            Assert.IsTrue(await connectionStore.IsTokenInUseAsync(id));
        }

        [TestMethod]
        public async Task When_disconnect_with_valid_token_Given_not_connected_Then_not_happens()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Send(new DisconnectSecretKeyCommand(id, token));

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

            var connectionStore = services.GetRequiredService<ISecretKeyConnectionStore>();
            Assert.IsFalse(await connectionStore.IsTokenInUseAsync(id));
        }
    }
}
