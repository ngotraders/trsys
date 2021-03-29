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

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_given_no_data_exists()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
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
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_SUBSCRIBER_TOKEN);

            var repository = server.Services.GetRequiredService<IOrderRepository>();
            await repository.SaveOrdersAsync(new[] {
                new Order() {
                    TicketNo = "1",
                    Symbol = "USDJPY",
                    OrderType = OrderType.BUY
                }
            });

            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("1:USDJPY:0", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_and_multiple_entities_given_multiple_orders_exists()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_SUBSCRIBER_TOKEN);

            var repository = server.Services.GetRequiredService<IOrderRepository>();
            await repository.SaveOrdersAsync(new[] {
                new Order() {
                    TicketNo = "1",
                    Symbol = "USDJPY",
                    OrderType = OrderType.BUY
                },
                new Order() {
                    TicketNo = "2",
                    Symbol = "EURUSD",
                    OrderType = OrderType.SELL
                }
            });

            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("1:USDJPY:0@2:EURUSD:1", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_unauthorized_given_invalid_token()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_ok_given_empty_string()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
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
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_PUBLISHER_TOKEN);

            var res = await client.PostAsync("/api/orders", new StringContent("1:USDJPY:0", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            var repository = server.Services.GetRequiredService<IOrderRepository>();
            var orders = await repository.All.ToListAsync();
            Assert.AreEqual(1, orders.Count);
        }

        [TestMethod]
        public async Task PostApiOrders_should_return_ok_given_multiple_orders()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_PUBLISHER_TOKEN);

            var res = await client.PostAsync("/api/orders", new StringContent("1:USDJPY:0@2:EURUSD:1", Encoding.UTF8, "text/plain"));
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
            var res = await client.PostAsync("/api/orders", new StringContent("1:USDJPY:0@2:EURUSD:1", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Unauthorized, res.StatusCode);
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
            public Task<SecretTokenInfo> FindInfoAsync(string token)
            {
                var tokenInfo = null as SecretTokenInfo;
                if (token == VALID_PUBLISHER_TOKEN)
                {
                    tokenInfo = new SecretTokenInfo()
                    {
                        Token = VALID_PUBLISHER_TOKEN,
                        SecretKey = "SECRETKEY",
                        KeyType = SecretKeyType.Publisher
                    };
                }
                else if (token == VALID_SUBSCRIBER_TOKEN)
                {
                    tokenInfo = new SecretTokenInfo()
                    {
                        Token = VALID_SUBSCRIBER_TOKEN,
                        SecretKey = "SECRETKEY",
                        KeyType = SecretKeyType.Subscriber
                    };
                }
                return Task.FromResult(tokenInfo);
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