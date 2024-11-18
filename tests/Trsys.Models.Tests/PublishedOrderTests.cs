using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure;
using Trsys.Models.Events;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Models.Tests
{
    [TestClass]
    public class PublishedOrderTests
    {
        [TestMethod]
        public void When_parse_Given_valid_order_string1()
        {
            var order = PublishedOrder.Parse("106224984:AUDJPY:0:1731935769:100.184:0.98");
            Assert.AreEqual(106224984, order.TicketNo);
            Assert.AreEqual("AUDJPY", order.Symbol);
            Assert.AreEqual("AUDJPY", order.OriginalSymbol);
            Assert.AreEqual(OrderType.Buy, order.OrderType);
            Assert.AreEqual(1731935769, order.Time);
            Assert.AreEqual(100.184m, order.Price);
            Assert.AreEqual(0.98m, order.Percentage);
        }

        [TestMethod]
        public void When_parse_Given_valid_order_string2()
        {
            var order = PublishedOrder.Parse("106224985:usdjpy#:1:1731935769:100:1");
            Assert.AreEqual(106224985, order.TicketNo);
            Assert.AreEqual("USDJPY", order.Symbol);
            Assert.AreEqual("usdjpy#", order.OriginalSymbol);
            Assert.AreEqual(OrderType.Sell, order.OrderType);
            Assert.AreEqual(1731935769, order.Time);
            Assert.AreEqual(100m, order.Price);
            Assert.AreEqual(1m, order.Percentage);
        }

        [TestMethod]
        public void When_parse_Given_valid_order_string3()
        {
            var order = PublishedOrder.Parse("106224986:AUDJPY.oj3m:0:1731935769:100.184:0.98");
            Assert.AreEqual(106224986, order.TicketNo);
            Assert.AreEqual("AUDJPY", order.Symbol);
            Assert.AreEqual("AUDJPY.oj3m", order.OriginalSymbol);
            Assert.AreEqual(OrderType.Buy, order.OrderType);
            Assert.AreEqual(1731935769, order.Time);
            Assert.AreEqual(100.184m, order.Price);
            Assert.AreEqual(0.98m, order.Percentage);
        }
    }
}
