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
using Trsys.Web.Data;
using Trsys.Web.Models;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class OrderApiTests
    {
        private const string VALID_TOKEN = "VALID_TOKEN";

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_given_no_data_exists()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_TOKEN);
            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_and_single_entity_given_single_order_exists()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_TOKEN);
            var res = await client.PostAsync("/api/orders", new StringContent("1:USDJPY:0", Encoding.UTF8, "text/plain"));
            res.EnsureSuccessStatusCode();

            res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("1:USDJPY:0", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_ok_and_multiple_entities_given_multiple_orders_exists()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_TOKEN);
            var res = await client.PostAsync("/api/orders", new StringContent("1:USDJPY:0@2:EURUSD:1", Encoding.UTF8, "text/plain"));
            res.EnsureSuccessStatusCode();

            res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("1:USDJPY:0@2:EURUSD:1", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetApiOrders_should_return_fobidden_given_invalid_token()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.Forbidden, res.StatusCode);
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
                                services.AddSingleton<ITokenValidator>(new MockTokenValidator());
                            }));
        }

        private class MockTokenValidator : ITokenValidator
        {
            public bool Validate(string token)
            {
                return token == VALID_TOKEN;
            }
        }
    }
}
