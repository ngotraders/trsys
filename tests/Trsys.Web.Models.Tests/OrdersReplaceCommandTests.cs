using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Models.Tests
{
    [TestClass]
    public class OrdersReplaceCommandTests
    {
        [TestMethod]
        public async Task When_replace_order_Given_no_order_is_present_Then_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", "description", true));
            await mediator.Send(new OrdersReplaceCommand(id, new[]
            {
                PublishedOrder.Parse("1:USDJPY:0:1617271883:100:20")
            }));
            await mediator.Send(new OrdersReplaceCommand(id, new[]
            {
                PublishedOrder.Parse("1:USDJPY:0:1617271883:100:20"),
                PublishedOrder.Parse("2:EURJPY:1:1617271884:50:98")
            }));
            await mediator.Send(new OrdersReplaceCommand(id, new[]
            {
                PublishedOrder.Parse("2:EURJPY:1:1617271884:50:98")
            }));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(7, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
            Assert.AreEqual(typeof(SecretKeyApproved), events[3].GetType());
            Assert.AreEqual(typeof(OrderPublisherOpenedOrder), events[4].GetType());
            Assert.AreEqual(1, ((OrderPublisherOpenedOrder)events[4]).Order.TicketNo);
            Assert.AreEqual(typeof(OrderPublisherOpenedOrder), events[5].GetType());
            Assert.AreEqual(2, ((OrderPublisherOpenedOrder)events[5]).Order.TicketNo);
            Assert.AreEqual(typeof(OrderPublisherClosedOrder), events[6].GetType());
            Assert.AreEqual(1, ((OrderPublisherClosedOrder)events[6]).TicketNo);
        }
    }
}
