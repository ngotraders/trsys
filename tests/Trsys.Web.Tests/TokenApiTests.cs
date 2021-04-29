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
using Trsys.Web.Models.SecretKeys;
using Trsys.Web.Services;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class TokenApiTests
    {
        [TestMethod]
        public async Task PostApiToken_should_return_ok_given_valid_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            var key = null as string;

            using (var scope = server.Services.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<SecretKeyService>();
                var result = await service.RegisterSecretKeyAsync(null, SecretKeyType.Subscriber, null);
                key = result.Key;
                await service.ApproveSecretKeyAsync(key);
            }
            var res = await client.PostAsync("/api/token", new StringContent(key, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            using (var scope = server.Services.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<SecretKeyService>();
                var secretKey = await service.FindBySecretKeyAsync(key);
                Assert.AreEqual(secretKey.ValidToken, await res.Content.ReadAsStringAsync());
            }
        }

        [TestMethod]
        public async Task PostApiToken_should_return_badrequest_given_not_exsisting_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            var res = await client.PostAsync("/api/token", new StringContent("INVALID_SECRET_KEY", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidSecretKey", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiToken_should_return_badrequest_given_invalid_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            var res = await client.PostAsync("/api/token", new StringContent("INVALID_SECRET_KEY", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidSecretKey", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiToken_should_return_badrequest_given_in_use_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();

            var service = server.Services.GetRequiredService<SecretKeyService>();
            var result = await service.RegisterSecretKeyAsync(null, SecretKeyType.Subscriber, null);
            var key = result.Key;
            await service.ApproveSecretKeyAsync(key);
            var res = await client.PostAsync("/api/token", new StringContent(key, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            await service.TouchSecretTokenAsync(key);

            res = await client.PostAsync("/api/token", new StringContent(key, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("SecretKeyInUse", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiTokenRelease_should_return_ok_given_valid_token_and_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();

            var service = server.Services.GetRequiredService<SecretKeyService>();
            var result = await service.RegisterSecretKeyAsync(null, SecretKeyType.Subscriber, null);
            var key = result.Key;
            await service.ApproveSecretKeyAsync(key);
            var tokenResult = await service.GenerateSecretTokenAsync(key);
            var token = tokenResult.Token;
            await service.TouchSecretTokenAsync(key);

            var store = server.Services.GetRequiredService<IAuthenticationTicketStore>();
            store.Add(token, PrincipalGenerator.Generate(key, SecretKeyType.Publisher));

            var res = await client.PostAsync("/api/token/" + token + "/release", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            Assert.IsNull(store.Find(token));
            var secretKey = await service.FindBySecretKeyAsync(key);
            Assert.IsFalse(secretKey.HasToken);
        }

        [TestMethod]
        public async Task PostApiTokenRelease_should_return_ok_given_valid_token()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();

            var token = Guid.NewGuid().ToString();
            var store = server.Services.GetRequiredService<IAuthenticationTicketStore>();
            store.Add(token, PrincipalGenerator.Generate("AnyKey", SecretKeyType.Publisher));

            var res = await client.PostAsync("/api/token/" + token + "/release", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            Assert.IsNull(store.Find(token));
        }

        [TestMethod]
        public async Task PostApiTokenRelease_should_return_badrequest_given_invalid_token()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            var res = await client.PostAsync("/api/token/INVALID_TOKEN/release", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidToken", await res.Content.ReadAsStringAsync());
        }

        private static TestServer CreateTestServer()
        {
            var databaseName = Guid.NewGuid().ToString();
            return new TestServer(new WebHostBuilder()
                            .UseConfiguration(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build())
                            .UseStartup<Startup>()
                            .ConfigureTestServices(services =>
                            {
                                services.AddSingleton(new TrsysContext(new DbContextOptionsBuilder<TrsysContext>().UseInMemoryDatabase(databaseName).Options));
                            }));
        }
    }
}
