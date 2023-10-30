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
    public class FetchOrderCommandTests
    {
        [TestMethod]
        public async Task When_published_order_fetched_by_subscriber_Given_no_orders_Then_nothing_happens()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            await mediator.Send(new FetchOrderCommand(id, Array.Empty<int>()));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
        }

        [TestMethod]
        public async Task When_published_order_fetched_by_subscriber_Given_single_order_Then_subscriber_order_opens()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            await mediator.Send(new FetchOrderCommand(id, new int[] { 1 }));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[3].GetType());
            Assert.AreEqual(1, ((OrderSubscriberOpenedOrder)events[3]).TicketNo);
        }

        [TestMethod]
        public async Task When_closed_order_fetched_by_subscriber_Given_single_order_Then_subscriber_order_closes()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            await mediator.Send(new FetchOrderCommand(id, new int[] { 1 }));
            await mediator.Send(new FetchOrderCommand(id, Array.Empty<int>()));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(5, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[3].GetType());
            Assert.AreEqual(1, ((OrderSubscriberOpenedOrder)events[3]).TicketNo);
            Assert.AreEqual(typeof(OrderSubscriberClosedOrder), events[4].GetType());
            Assert.AreEqual(1, ((OrderSubscriberClosedOrder)events[4]).TicketNo);
        }

        [TestMethod]
        public async Task When_published_order_fetched_by_subscriber_Given_multiple_orders_Then_subscriber_order_opens()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            await mediator.Send(new FetchOrderCommand(id, new int[] { 1, 2 }));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(5, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[3].GetType());
            Assert.AreEqual(1, ((OrderSubscriberOpenedOrder)events[3]).TicketNo);
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[4].GetType());
            Assert.AreEqual(2, ((OrderSubscriberOpenedOrder)events[4]).TicketNo);
        }

        [TestMethod]
        public async Task When_closed_order_fetched_by_subscriber_Given_multiple_orders_Then_subscriber_order_closes()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            await mediator.Send(new FetchOrderCommand(id, new int[] { 1, 2 }));
            await mediator.Send(new FetchOrderCommand(id, new int[] { 2 }));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(6, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[3].GetType());
            Assert.AreEqual(1, ((OrderSubscriberOpenedOrder)events[3]).TicketNo);
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[4].GetType());
            Assert.AreEqual(2, ((OrderSubscriberOpenedOrder)events[4]).TicketNo);
            Assert.AreEqual(typeof(OrderSubscriberClosedOrder), events[5].GetType());
            Assert.AreEqual(1, ((OrderSubscriberClosedOrder)events[5]).TicketNo);
        }

        [TestMethod]
        public async Task When_opened_and_closed_order_fetched_by_subscriber_at_once_Then_subscriber_order_closes_and_opens()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            await mediator.Send(new FetchOrderCommand(id, new int[] { 1, 2 }));
            await mediator.Send(new FetchOrderCommand(id, new int[] { 3,4 }));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(9, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[3].GetType());
            Assert.AreEqual(1, ((OrderSubscriberOpenedOrder)events[3]).TicketNo);
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[4].GetType());
            Assert.AreEqual(2, ((OrderSubscriberOpenedOrder)events[4]).TicketNo);
            Assert.AreEqual(typeof(OrderSubscriberClosedOrder), events[5].GetType());
            Assert.AreEqual(1, ((OrderSubscriberClosedOrder)events[5]).TicketNo);
            Assert.AreEqual(typeof(OrderSubscriberClosedOrder), events[6].GetType());
            Assert.AreEqual(2, ((OrderSubscriberClosedOrder)events[6]).TicketNo);
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[7].GetType());
            Assert.AreEqual(3, ((OrderSubscriberOpenedOrder)events[7]).TicketNo);
            Assert.AreEqual(typeof(OrderSubscriberOpenedOrder), events[8].GetType());
            Assert.AreEqual(4, ((OrderSubscriberOpenedOrder)events[8]).TicketNo);
        }
    }
}
