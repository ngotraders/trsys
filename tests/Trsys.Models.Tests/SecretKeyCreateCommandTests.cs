using CQRSlite.Domain;
using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure;
using Trsys.Models.Events;
using Trsys.Models.WriteModel.Commands;
using Trsys.Models.WriteModel.Extensions;

namespace Trsys.Models.Tests
{
    [TestClass]
    public class SecretKeyCreateCommandTests
    {
        [TestMethod]
        public async Task When_KeyType_Key_and_Description_specified_Then_creation_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, "TEST_KEY", "description"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("TEST_KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
        }

        [TestMethod]
        public async Task When_KeyType_is_specified_and_approve_is_true_Then_creation_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, null, null, true));

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
        public async Task When_KeyType_is_not_specified_and_approve_is_true_Then_creation_fails()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await mediator.Send(new SecretKeyCreateCommand(null, null, null, true)));
        }

        [TestMethod]
        public async Task When_KeyType_Key_and_Description_is_not_specified_Then_creation_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(null, null, null));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
        }

        [TestMethod]
        public async Task Given_Key_already_exists_Then_no_event_created()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(null, "TEST_KEY", null));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await mediator.Send(new SecretKeyCreateCommand(null, "TEST_KEY", null)));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("TEST_KEY", ((SecretKeyCreated)events[0]).Key);
        }

        [TestMethod]
        public async Task Given_Key_already_exists_and_not_aggregate_created_Then_creation_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var repository = services.GetRequiredService<IRepository>();
            var state = await repository.GetWorldState();
            state.GenerateSecretKeyIdIfNotExists("TEST_KEY", out var id);
            await repository.Save(state);
            var mediator = services.GetRequiredService<IMediator>();
            await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, "TEST_KEY", "description"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("TEST_KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
        }
    }
}
