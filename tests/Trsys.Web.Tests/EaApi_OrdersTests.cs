using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class EaApi_OrdersTests
    {
        private const string VALID_KEY = "VALID_KEY";
        private const string VALID_VERSION = "20211109";

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_given_no_data_exists()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            var res = await client.GetAsync("/api/ea/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_and_single_entity_given_single_order_exists()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            await mediator.Send(new OrdersReplaceCommand(id, new[] {
                new PublishedOrder() {
                    TicketNo = 1,
                    Symbol = "USDJPY",
                    OrderType = OrderType.Buy,
                    Time = 1617271883,
                    Price = 1,
                    Percentage = 0.1m,
                }
            }));

            var res = await client.GetAsync("/api/ea/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("1:USDJPY:0:1617271883:1:0.1", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_and_multiple_entities_given_multiple_orders_exists()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            await mediator.Send(new OrdersReplaceCommand(id, new[] {
                new PublishedOrder() {
                    TicketNo = 1,
                    Symbol = "USDJPY",
                    OrderType = OrderType.Buy,
                    Time = 1617271883,
                    Price = 1.2m,
                    Percentage = 0.4m,
                },
                new PublishedOrder() {
                    TicketNo = 2,
                    Symbol = "EURUSD",
                    OrderType = OrderType.Sell,
                    Time = 1617271884,
                    Price = 0,
                    Percentage = 0.5m,
                }
            }));

            var res = await client.GetAsync("/api/ea/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("1:USDJPY:0:1617271883:1.2:0.4@2:EURUSD:1:1617271884:0:0.5", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_not_modified_given_cache_exists()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            await mediator.Send(new OrdersReplaceCommand(id, new[] {
                new PublishedOrder() {
                    TicketNo = 1,
                    Symbol = "USDJPY",
                    OrderType = OrderType.Buy,
                    Time = 1617271872,
                    Price = 1,
                    Percentage = 0.4m,
                },
                new PublishedOrder() {
                    TicketNo = 2,
                    Symbol = "EURUSD",
                    OrderType = OrderType.Sell,
                    Time = 1617271873,
                    Price = 180,
                    Percentage = 0.4m,
                }
            }));

            var res1 = await client.GetAsync("/api/ea/orders");
            Assert.AreEqual(HttpStatusCode.OK, res1.StatusCode);
            Assert.AreEqual("1:USDJPY:0:1617271872:1:0.4@2:EURUSD:1:1617271873:180:0.4", await res1.Content.ReadAsStringAsync());

            client.DefaultRequestHeaders.Add("If-None-Match", "\"INVALID_TAG\"");
            var res2 = await client.GetAsync("/api/ea/orders");
            Assert.AreEqual(HttpStatusCode.OK, res2.StatusCode);
            Assert.AreEqual("1:USDJPY:0:1617271872:1:0.4@2:EURUSD:1:1617271873:180:0.4", await res2.Content.ReadAsStringAsync());
            Assert.AreEqual(res1.Headers.ETag, res2.Headers.ETag);

            client.DefaultRequestHeaders.Add("If-None-Match", res2.Headers.ETag.Tag);
            var res3 = await client.GetAsync("/api/ea/orders");
            Assert.AreEqual(HttpStatusCode.NotModified, res3.StatusCode);
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_unauthorized_given_invalid_token()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", "InvalidToken");

            var res = await client.GetAsync("/api/ea/orders");
            Assert.AreEqual(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_bad_request_given_invalid_version()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Ea-Version", "20210330");
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);

            var res = await client.GetAsync("/api/ea/orders");
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidVersion", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_ok_given_empty_string()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);

            var res = await client.PostAsync("/api/ea/orders", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            var orders = await mediator.Send(new GetPublishedOrders());
            Assert.AreEqual(0, orders.Count);
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_ok_given_single_order()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);

            var res = await client.PostAsync("/api/ea/orders", new StringContent("1:USDJPY0jp:0:1:2:3", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            var orders = await mediator.Send(new GetPublishedOrders());

            Assert.AreEqual(1, orders.Count);
            Assert.AreEqual(1, orders[0].TicketNo);
            Assert.AreEqual("USDJPY0jp", orders[0].Symbol);
            Assert.AreEqual(OrderType.Buy, orders[0].OrderType);
            Assert.AreEqual(1, orders[0].Time);
            Assert.AreEqual(2, orders[0].Price);
            Assert.AreEqual(3, orders[0].Percentage);
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_ok_given_multiple_orders()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);

            var res = await client.PostAsync("/api/ea/orders", new StringContent("1:USDJPY:0:1:0.2:0.3@2:EURUSD:1:100:2.00:3", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            var orders = await mediator.Send(new GetPublishedOrders());
            Assert.AreEqual(2, orders.Count);
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_unauthorized_given_invalid_token()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, VALID_KEY, null, true));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", "InvalidToken");

            var res = await client.PostAsync("/api/ea/orders", new StringContent("1:USDJPY:0:1:0.2:0.3@2:EURUSD:1:100:2.00:3", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_bad_request_given_invalid_version()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);

            var res = await client.PostAsync("/api/ea/orders", new StringContent("1:USDJPY:0:1:0.2:0.3@2:EURUSD:1:100:2.00:3", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("X-Ea-Version is not set.", await res.Content.ReadAsStringAsync());
        }
    }
}
