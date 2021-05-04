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
using Trsys.Web.Authentication;
using Trsys.Web.Data;
using Trsys.Web.Infrastructure;
using Trsys.Web.Infrastructure.InMemory;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class LogsApiTests
    {
        private const string VALID_KEY = "VALID_KEY";
        private const string VALID_SUBSCRIBER_TOKEN = "VALID_SUBSCRIBER_TOKEN";
        private const string VALID_PUBLISHER_TOKEN = "VALID_PUBLISHER_TOKEN";
        private const string VALID_VERSION = "20210331";

        [TestMethod]
        public async Task PostLog_should_return_ok_given_empty_string()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_PUBLISHER_TOKEN);

            var res = await client.PostAsync("/api/logs", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);

            var repository = server.Services.GetRequiredService<IEventRepository>();
            var events = await repository.SearchAllAsync();
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual($"ea/{VALID_KEY}/Log", events.First().EventType);
            Assert.IsNull(events.First().Data);
        }
        [TestMethod]
        public async Task PostLog_should_return_ok_given_non_empty_string()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", VALID_PUBLISHER_TOKEN);

            var res = await client.PostAsync("/api/logs", new StringContent("NonEmpty", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);

            var repository = server.Services.GetRequiredService<IEventRepository>();
            var events = await repository.SearchAllAsync();
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual($"ea/{VALID_KEY}/Log", events.First().EventType);
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
                                services.AddSingleton<IAuthenticationTicketStore>(new MockAuthenticationTicketStore());
                            }));

        }

        private class MockAuthenticationTicketStore : InMemoryAuthenticationTicketStore
        {
            public MockAuthenticationTicketStore()
            {
                AddAsync(VALID_PUBLISHER_TOKEN, SecretKeyAuthenticationTicketFactory.Create(VALID_KEY, SecretKeyType.Publisher));
                AddAsync(VALID_SUBSCRIBER_TOKEN, SecretKeyAuthenticationTicketFactory.Create(VALID_KEY, SecretKeyType.Subscriber));
            }
        }
    }
}
