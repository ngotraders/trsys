using MediatR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class AdminApi_SecretKeysTests
    {
        [TestMethod]
        public async Task Index_search_secret_keys()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, "key1", null));
            await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, "key2", null));
            await client.LoginAsync();

            var res = await client.GetAsync("/api/admin/secret-keys");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(2, int.Parse(res.Headers.GetValues("X-Total-Count").First()));
            var retObj = await res.Content.ReadAsStringAsync();
            Assert.IsTrue(retObj.Contains("key1"));
            Assert.IsTrue(retObj.Contains("key2"));
        }

        [TestMethod]
        public async Task Index_search_secret_keys_with_pagenation()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, "key1", null));
            await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, "key2", null));
            await client.LoginAsync();

            var res = await client.GetAsync("/api/admin/secret-keys?_start=0&_end=1");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(2, int.Parse(res.Headers.GetValues("X-Total-Count").First()));
            var retObj = await res.Content.ReadAsStringAsync();
            Assert.IsTrue(retObj.Contains("key1"));
            Assert.IsFalse(retObj.Contains("key2"));

            res = await client.GetAsync("/api/admin/secret-keys?_start=1&_end=2");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(2, int.Parse(res.Headers.GetValues("X-Total-Count").First()));
            retObj = await res.Content.ReadAsStringAsync();
            Assert.IsFalse(retObj.Contains("key1"));
            Assert.IsTrue(retObj.Contains("key2"));
        }

        [TestMethod]
        public async Task GetKey_should_return_ok_if_specified_secret_key_exists()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, "key1", null));
            await client.LoginAsync();

            // Admin ユーザーが存在するので、1ページ目は2件
            var res = await client.GetAsync("/api/admin/secret-keys/" + id);
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var retObj = await res.Content.ReadAsStringAsync();
            Assert.IsTrue(retObj.Contains("key1"));
        }

        [TestMethod]
        public async Task GetKey_should_return_not_found_if_specified_secret_key_not_exists()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();
            var res = await client.GetAsync($"/api/admin/secret-keys/{Guid.NewGuid()}");
            Assert.AreEqual(HttpStatusCode.NotFound, res.StatusCode);
        }

        [TestMethod]
        public async Task PostKey_should_return_bad_request_given_secret_key_type_not_specified()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();

            var res = await client.PostAsync("/api/admin/secret-keys", new StringContent(JsonConvert.SerializeObject(new
            {
                KeyType = default(int?),
                Description = default(string),
            }), Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [TestMethod]
        public async Task PostKey_should_return_ok_given_is_approved_is_false()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();

            var res = await client.PostAsync("/api/admin/secret-keys", new StringContent(JsonConvert.SerializeObject(new
            {
                KeyType = 1,
                Description = default(string),
            }), Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var obj = JsonConvert.DeserializeObject<JObject>(await res.Content.ReadAsStringAsync());
            Assert.AreEqual(SecretKeyType.Publisher, obj.Property("keyType").Value.ToObject<SecretKeyType?>());
            Assert.IsNotNull(obj.Property("key").Value.ToObject<string>());
            Assert.IsNull(obj.Property("description").Value.ToObject<string>());
            Assert.AreEqual(false, obj.Property("isApproved").ToObject<bool>());
        }

        [TestMethod]
        public async Task PostKey_should_return_ok_given_secret_key_type_specified()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();

            var res = await client.PostAsync("/api/admin/secret-keys", new StringContent(JsonConvert.SerializeObject(new
            {
                KeyType = 1,
                Description = default(string),
                IsApproved = true,
            }), Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var obj = JsonConvert.DeserializeObject<JObject>(await res.Content.ReadAsStringAsync());
            Assert.AreEqual(SecretKeyType.Publisher, obj.Property("keyType").Value.ToObject<SecretKeyType?>());
            Assert.IsNotNull(obj.Property("key").Value.ToObject<string>());
            Assert.IsNull(obj.Property("description").Value.ToObject<string>());
            Assert.AreEqual(true, obj.Property("isApproved").ToObject<bool>());
        }
    }
}
