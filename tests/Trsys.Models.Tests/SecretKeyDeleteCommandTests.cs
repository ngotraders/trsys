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

namespace Trsys.Models.Tests
{
    [TestClass]
    public class SecretKeyDeleteCommandTests
    {
        [TestMethod]
        public async Task Given_approved_Then_deletion_fails()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, null, null, true));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await mediator.Send(new SecretKeyDeleteCommand(id)));

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
        public async Task Given_not_approved_Then_deletion_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(null, null, null));
            await mediator.Send(new SecretKeyDeleteCommand(id));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyDeleted), events[1].GetType());
        }

        [TestMethod]
        public async Task Given_connected_Then_deletion_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, null, null, true));
            var token = await mediator.Send(new SecretKeyGenerateSecretTokenCommand(id));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await mediator.Send(new SecretKeyDeleteCommand(id)));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[3].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[3]).Token);
        }
    }
}
