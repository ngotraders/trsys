using MediatR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class EaApi_LogsTests
    {
        private const string VALID_KEY = "VALID_KEY";
        private const string VALID_VERSION = "20211109";

        [TestMethod]
        public async Task PostLog_should_return_accepted_given_empty_string()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, VALID_KEY, null, true));
            var token = await mediator.Send(new SecretKeyGenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);

            var res = await client.PostAsync("/api/ea/logs", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);
        }

        [TestMethod]
        public async Task PostLog_should_return_accepted_given_non_empty_string()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, VALID_KEY, null, true));
            var token = await mediator.Send(new SecretKeyGenerateSecretTokenCommand(id));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);

            var res = await client.PostAsync("/api/ea/logs", new StringContent("1:DEBUG:NonEmpty", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);
        }

        [TestMethod]
        public async Task PostLog_should_return_accepted_given_invalid_token()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, VALID_KEY, null, true));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);
            client.DefaultRequestHeaders.Add("X-Secret-Token", "InvalidToken");

            var res = await client.PostAsync("/api/ea/logs", new StringContent("1:DEBUG:NonEmpty", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);
        }

        [TestMethod]
        public async Task PostLog_should_return_accepted_given_without_token()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetRequiredService<IMediator>();
            await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Publisher, VALID_KEY, null, true));

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            var res = await client.PostAsync("/api/ea/logs", new StringContent("1:DEBUG:NonEmpty", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);
        }

        [TestMethod]
        public async Task PostLog_should_return_accepted_given_unknown_key()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            client.DefaultRequestHeaders.Add("X-Ea-Id", "InvalidKey");
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Publisher");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            var res = await client.PostAsync("/api/ea/logs", new StringContent("1:DEBUG:NonEmpty", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.Accepted, res.StatusCode);
        }
    }
}
