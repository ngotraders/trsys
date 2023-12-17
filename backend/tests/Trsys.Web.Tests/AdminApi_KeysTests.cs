using MediatR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class AdminApi_KeysTests
    {
        [TestMethod]
        public async Task PostKey_should_return_bad_request_given_key_type_not_specified()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();

            var res = await client.PostAsync("/api/admin/keys", new StringContent(JsonConvert.SerializeObject(new
            {
                KeyType = default(int?),
                Description = default(string),
            }), Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [TestMethod]
        public async Task PostKey_should_return_created_given_is_approved_is_false()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();

            var res = await client.PostAsync("/api/admin/keys", new StringContent(JsonConvert.SerializeObject(new
            {
                KeyType = 1,
                Description = default(string),
            }), Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.Created, res.StatusCode);
            var key = JsonConvert.DeserializeObject<JObject>(await res.Content.ReadAsStringAsync()).Property("key").Value;
            Assert.IsNotNull(key);

            var keyRes = await client.GetAsync($"/api/admin/keys/{key}");
            Assert.AreEqual(HttpStatusCode.OK, keyRes.StatusCode);
            var retObj = JsonConvert.DeserializeObject<JObject>(await keyRes.Content.ReadAsStringAsync());
            Assert.AreEqual(false, retObj.Property("isApproved").Value);
        }

        [TestMethod]
        public async Task PostKey_should_return_created_given_key_type_specified()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();

            var res = await client.PostAsync("/api/admin/keys", new StringContent(JsonConvert.SerializeObject(new
            {
                KeyType = 1,
                Description = default(string),
                IsApproved = true,
            }), Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.Created, res.StatusCode);
            var key = JsonConvert.DeserializeObject<JObject>(await res.Content.ReadAsStringAsync()).Property("key").Value;
            Assert.IsNotNull(key);

            var keyRes = await client.GetAsync($"/api/admin/keys/{key}");
            Assert.AreEqual(HttpStatusCode.OK, keyRes.StatusCode);
            var retObj = JsonConvert.DeserializeObject<JObject>(await keyRes.Content.ReadAsStringAsync());
            Assert.AreEqual(true, retObj.Property("isApproved").Value);
        }

        [TestMethod]
        public async Task GetKey_should_return_bad_request_given_key_type_not_specified()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();
            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, null, null));
            var key = await mediator.Send(new GetSecretKey(id));

            var res = await client.GetAsync($"/api/admin/keys/{key.Key}");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(key.Key, JsonConvert.DeserializeObject<JObject>(await res.Content.ReadAsStringAsync()).Property("key").Value);
        }
    }
}
