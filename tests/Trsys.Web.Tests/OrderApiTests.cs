using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trsys.Web.Authentication;
using Trsys.Web.Data;
using Trsys.Web.Models;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class OrderApiTests
    {
        private const string VALID_SUBSCRIBER_TOKEN = "VALID_SUBSCRIBER_TOKEN";
        private const string VALID_PUBLISHER_TOKEN = "VALID_PUBLISHER_TOKEN";
        private const string VALID_VERSION = "20210331";

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_given_no_data_exists()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_SUBSCRIBER_TOKEN);
            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_and_single_entity_given_single_order_exists()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_SUBSCRIBER_TOKEN);

            var repository = server.Services.GetRequiredService<IOrderRepository>();
            await repository.SaveOrdersAsync(new[] {
                new Order() {
                    TicketNo = 1,
                    Symbol = "USDJPY",
                    OrderType = OrderType.BUY,
                    Price = 1,
                    Lots = 2,
                    Time = DateTimeOffset.Parse("2021-04-01T10:11:23.000Z"),
                }
            });

            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("1:USDJPY:0:1:2:1617271883", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_and_multiple_entities_given_multiple_orders_exists()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_SUBSCRIBER_TOKEN);

            var repository = server.Services.GetRequiredService<IOrderRepository>();
            await repository.SaveOrdersAsync(new[] {
                new Order() {
                    TicketNo = 1,
                    Symbol = "USDJPY",
                    OrderType = OrderType.BUY,
                    Price = 1.2m,
                    Lots = 2.2m,
                    Time = DateTimeOffset.Parse("2021-04-01T10:11:23.000Z"),
                },
                new Order() {
                    TicketNo = 2,
                    Symbol = "EURUSD",
                    OrderType = OrderType.SELL,
                    Price = 0,
                    Lots = 0,
                    Time = DateTimeOffset.Parse("2021-04-01T10:11:24.000Z"),
                }
            });

            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("1:USDJPY:0:1.2:2.2:1617271883@2:EURUSD:1:0:0:1617271884", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_not_modified_given_cache_exists()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_SUBSCRIBER_TOKEN);

            var repository = server.Services.GetRequiredService<IOrderRepository>();
            await repository.SaveOrdersAsync(new[] {
                new Order() {
                    TicketNo = 1,
                    Symbol = "USDJPY",
                    OrderType = OrderType.BUY,
                    Price = 1,
                    Lots = 2,
                    Time = DateTimeOffset.Parse("2021-04-01T10:11:12.000Z")
                },
                new Order() {
                    TicketNo = 2,
                    Symbol = "EURUSD",
                    OrderType = OrderType.SELL,
                    Price = 180,
                    Lots = 20,
                    Time = DateTimeOffset.Parse("2021-04-01T10:11:13.000Z")
                }
            });

            var res1 = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res1.StatusCode);
            Assert.AreEqual("1:USDJPY:0:1:2:1617271872@2:EURUSD:1:180:20:1617271873", await res1.Content.ReadAsStringAsync());

            client.DefaultRequestHeaders.Add("If-None-Match", "\"INVALID_TAG\"");
            var res2 = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res2.StatusCode);
            Assert.AreEqual("1:USDJPY:0:1:2:1617271872@2:EURUSD:1:180:20:1617271873", await res2.Content.ReadAsStringAsync());
            Assert.AreEqual(res1.Headers.ETag, res2.Headers.ETag);

            client.DefaultRequestHeaders.Add("If-None-Match", res2.Headers.ETag.Tag);
            var res3 = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.NotModified, res3.StatusCode);
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_unauthorized_given_invalid_token()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_bad_request_given_invalid_version()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_SUBSCRIBER_TOKEN);
            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidVersion", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_ok_given_empty_string()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_PUBLISHER_TOKEN);

            var res = await client.PostAsync("/api/orders", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            var repository = server.Services.GetRequiredService<IOrderRepository>();
            var orders = await repository.All.ToListAsync();
            Assert.AreEqual(0, orders.Count);
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_ok_given_single_order()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_PUBLISHER_TOKEN);

            var res = await client.PostAsync("/api/orders", new StringContent("1:USDJPY:0:1:2:3", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            var repository = server.Services.GetRequiredService<IOrderRepository>();
            var orders = await repository.All.ToListAsync();

            Assert.AreEqual(1, orders.Count);
            Assert.AreEqual(1, orders[0].TicketNo);
            Assert.AreEqual("USDJPY", orders[0].Symbol);
            Assert.AreEqual(OrderType.BUY, orders[0].OrderType);
            Assert.AreEqual(1, orders[0].Price);
            Assert.AreEqual(2, orders[0].Lots);
            Assert.AreEqual(3, orders[0].Time.ToUnixTimeSeconds());
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_ok_given_multiple_orders()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_PUBLISHER_TOKEN);

            var res = await client.PostAsync("/api/orders", new StringContent("1:USDJPY:0:0.1:1.2:1@2:EURUSD:1:1.2:2.00:100", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            var repository = server.Services.GetRequiredService<IOrderRepository>();
            var orders = await repository.All.ToListAsync();
            Assert.AreEqual(2, orders.Count);
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_unauthorized_given_invalid_token()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            var res = await client.PostAsync("/api/orders", new StringContent("1:USDJPY:0:2@2:EURUSD:1:0.023", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_bad_request_given_invalid_version()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_PUBLISHER_TOKEN);
            var res = await client.PostAsync("/api/orders", new StringContent("1:USDJPY:0:120.23@2:EURUSD:1:0.0001", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidVersion", await res.Content.ReadAsStringAsync());
        }


        private static TestServer CreateTestServer()
        {
            var databaseName = Guid.NewGuid().ToString();
            return new TestServer(new WebHostBuilder()
                            .UseConfiguration(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build())
                            .UseStartup<Startup>()
                            .ConfigureServices(services =>
                            {
                                services.AddDbContext<TrsysContext>(options => options.UseInMemoryDatabase(databaseName));
                            })
                            .ConfigureTestServices(services =>
                            {
                                services.AddSingleton<ISecretTokenStore>(new MockTokenStore());
                            }));

        }

        private class MockTokenStore : ISecretTokenStore
        {
            public Task<SecretTokenInfo> FindInfoUpdatingAccessTimeAsync(string token)
            {
                var tokenInfo = null as SecretTokenInfo;
                if (token == VALID_PUBLISHER_TOKEN)
                {
                    tokenInfo = new SecretTokenInfo("SECRETKEY", SecretKeyType.Publisher, VALID_PUBLISHER_TOKEN);
                }
                else if (token == VALID_SUBSCRIBER_TOKEN)
                {
                    tokenInfo = new SecretTokenInfo("SECRETKEY", SecretKeyType.Subscriber, VALID_SUBSCRIBER_TOKEN);
                }
                return Task.FromResult(tokenInfo);
            }

            public Task<SecretTokenInfo> FindInfoAsync(string token)
            {
                throw new NotImplementedException();
            }

            public Task<string> RegisterTokenAsync(string secretKey, SecretKeyType keyType)
            {
                throw new NotImplementedException();
            }

            public Task UnregisterAsync(string token)
            {
                throw new NotImplementedException();
            }
        }
    }
}
