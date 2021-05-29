using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Infrastructure;
using Trsys.Web.Infrastructure.Caching;
using Trsys.Web.Infrastructure.Caching.InMemory;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.SecretKeys;
using Trsys.Web.Services;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class LogsApiTests
    {
        private const string VALID_KEY = "VALID_KEY";
        private const string VALID_VERSION = "20210331";

        [TestMethod]
        public async Task PostLog_should_return_accepted_given_empty_string()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            var service = server.Services.GetRequiredService<SecretKeyService>();
            await service.RegisterSecretKeyAsync(VALID_KEY, SecretKeyType.Publisher, null);
            await service.ApproveSecretKeyAsync(VALID_KEY);
            var result = await service.GenerateSecretTokenAsync(VALID_KEY);

            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", result.Token);

            var res = await client.PostAsync("/api/logs", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);

            await Task.Delay(1);
            var repository = server.Services.GetRequiredService<IEventRepository>();
            var events = await repository.SearchAllAsync();
            Assert.AreEqual(0, events.Count);
        }
        [TestMethod]
        public async Task PostLog_should_return_ok_given_non_empty_string()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();

            var service = server.Services.GetRequiredService<SecretKeyService>();
            await service.RegisterSecretKeyAsync(VALID_KEY, SecretKeyType.Publisher, null);
            await service.ApproveSecretKeyAsync(VALID_KEY);

            var result = await service.GenerateSecretTokenAsync(VALID_KEY);
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", result.Token);

            var res = await client.PostAsync("/api/logs", new StringContent("NonEmpty", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);

            var repository = server.Services.GetRequiredService<IEventRepository>();
            var events = await repository.SearchAllAsync();
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual($"ea/{VALID_KEY}", events.First().Source);
            Assert.AreEqual("Log", events.First().EventType);
            Assert.AreEqual("NonEmpty", events.First().Data);
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
                                services.AddRepositories();
                            }));
        }
    }
}
