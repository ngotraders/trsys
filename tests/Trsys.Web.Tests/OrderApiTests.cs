using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class OrderApiTests
    {
        [TestMethod]
        public async Task GetApiOrders_should_return_ok_if_no_data_exists()
        {
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            var client = server.CreateClient();
            var res = await client.GetAsync("/api/orders");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual("", await res.Content.ReadAsStringAsync());
        }
    }
}
