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
                var repository = scope.ServiceProvider.GetRequiredService<ISecretKeyRepository>();
                var secretKey = await repository.CreateNewSecretKeyAsync(SecretKeyType.Subscriber);
                key = secretKey.Key;
                secretKey.Approve();
                await repository.SaveAsync(secretKey);
            }
            var res = await client.PostAsync("/api/token", new StringContent(key, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            using (var scope = server.Services.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ISecretKeyRepository>();
                var secretKey = await repository.FindBySecretKeyAsync(key);
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

            var repository = server.Services.GetRequiredService<ISecretKeyRepository>();
            var secretKey = await repository.CreateNewSecretKeyAsync(SecretKeyType.Subscriber);
            secretKey.Approve();
            await repository.SaveAsync(secretKey);

            var res = await client.PostAsync("/api/token", new StringContent(secretKey.Key, Encoding.UTF8, "text/plain"));
            var tokenStore = server.Services.GetRequiredService<ISecretTokenStore>();
            await tokenStore.FindInfoAsync(await res.Content.ReadAsStringAsync());
            res = await client.PostAsync("/api/token", new StringContent(secretKey.Key, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("SecretKeyInUse", await res.Content.ReadAsStringAsync());
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
                            }));
        }
    }
}
