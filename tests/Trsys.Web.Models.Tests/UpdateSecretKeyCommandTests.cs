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
    public class UpdateSecretKeyCommandTests
    {
        [TestMethod]
        public async Task When_KeyType_and_Description_not_specified_Then_modification_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(null, null, null));
            await mediator.Send(new UpdateSecretKeyCommand(id, null, null));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
        }

        [TestMethod]
        public async Task When_KeyType_Key_and_Description_is_specified_Then_modification_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(null, null, null));
            await mediator.Send(new UpdateSecretKeyCommand(id, SecretKeyType.Publisher, "description"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
        }

        [TestMethod]
        public async Task When_KeyType_is_specified_and_approve_is_true_Then_modification_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(null, null, null));
            await mediator.Send(new UpdateSecretKeyCommand(id, SecretKeyType.Publisher, null, true));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
        }

        [TestMethod]
        public async Task Given_approved_When_KeyType_not_specified_Then_modification_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, null, null, true));
            await mediator.Send(new UpdateSecretKeyCommand(id, null, "description", null));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[3].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[3]).Description);
        }

        [TestMethod]
        public async Task Given_approved_When_KeyType_changing_Then_modification_fails()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, null, null, true));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await mediator.Send(new UpdateSecretKeyCommand(id, SecretKeyType.Subscriber, null, true)));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
        }

        [TestMethod]
        public async Task Given_approved_When_approve_is_false_Then_modification_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, null, null, true));
            await mediator.Send(new UpdateSecretKeyCommand(id, SecretKeyType.Publisher, null, false));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyRevoked), events[3].GetType());
        }

        [TestMethod]
        public async Task Given_approved_and_connected_When_revoke_Then_connection_closed()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, null, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Publish(new TokenTouched(token));
            await mediator.Send(new UpdateSecretKeyCommand(id, SecretKeyType.Publisher, null, false));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(6, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[3].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenInvalidated), events[4].GetType());
            Assert.AreEqual(typeof(SecretKeyRevoked), events[5].GetType());

            var connectionStore = services.GetRequiredService<ITokenConnectionManager>();
            Assert.IsFalse(await connectionStore.IsTokenInUseAsync(token));
        }
    }
}
