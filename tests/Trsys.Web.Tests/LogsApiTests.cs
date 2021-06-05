using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;

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

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);

            var res = await client.PostAsync("/api/logs", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);

            await Task.Delay(1);
            var events = await mediator.Send(new GetLogs());
            Assert.AreEqual(0, events.Count());
        }
        [TestMethod]
        public async Task PostLog_should_return_ok_given_non_empty_string()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, VALID_KEY, null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);

            var res = await client.PostAsync("/api/logs", new StringContent("1:DEBUG:NonEmpty", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);

            var events = await mediator.Send(new GetLogs());
            Assert.AreEqual(1, events.Count());
            Assert.AreEqual(VALID_KEY, events.First().Key);
            Assert.AreEqual("DEBUG", events.First().LogType);
            Assert.AreEqual("NonEmpty", events.First().Data);
        }

        private static TestServer CreateTestServer()
        {
            return new TestServer(new WebHostBuilder()
                .UseConfiguration(
                    new ConfigurationBuilder()
                    .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Trsys.Web:PasswordSalt", "salt"), }).Build()
                 )
                .UseStartup<Startup>());
        }
    }
}
