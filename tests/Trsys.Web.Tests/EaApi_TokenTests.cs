using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;
using Trsys.Models.WriteModel.Notifications;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class EaApi_TokenTests
    {
        private const string VALID_KEY = "VALID_KEY";
        private const string VALID_VERSION = "20211109";

        [TestMethod]
        public async Task PostApiToken_should_return_ok_given_valid_secret_key()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
                await mediator.Send(new SecretKeyGenerateSecretTokenCommand(id));
            }
            var res = await client.PostAsync("/api/ea/token/generate", new StringContent(VALID_KEY, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                var secretKey = await mediator.Send(new FindBySecretKey(VALID_KEY));
                Assert.AreEqual(secretKey.Token, await res.Content.ReadAsStringAsync());
            }
        }

        [TestMethod]
        public async Task PostApiToken_should_return_badrequest_given_not_exsisting_secret_key()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            var res = await client.PostAsync("/api/ea/token/generate", new StringContent("INVALID_SECRET_KEY", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidSecretKey", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiToken_should_return_badrequest_given_invalid_secret_key()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Subscriber, VALID_KEY, null));
            }

            var res = await client.PostAsync("/api/ea/token/generate", new StringContent(VALID_KEY, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidSecretKey", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiToken_should_return_badrequest_given_in_use_secret_key()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
                var token = await mediator.Send(new SecretKeyGenerateSecretTokenCommand(id));
                await mediator.Publish(new SecretKeyConnected(id, "NORMAL"));
            }

            var res = await client.PostAsync("/api/ea/token/generate", new StringContent(VALID_KEY, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("SecretKeyInUse", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiTokenRelease_should_return_ok_given_valid_token_and_secret_key()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var token = default(string);
            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
                token = await mediator.Send(new SecretKeyGenerateSecretTokenCommand(id));
                await mediator.Publish(new SecretKeyConnected(id, "NORMAL"));
            }

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Secret-Token", token);
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            var res = await client.PostAsync("/api/ea/token/release", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                Assert.IsNull(await mediator.Send(new FindByCurrentToken(token)));
            }
        }

        [TestMethod]
        public async Task PostApiTokenRelease_should_return_badrequest_given_invalid_token()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            client.DefaultRequestHeaders.Add("X-Ea-Id", VALID_KEY);
            client.DefaultRequestHeaders.Add("X-Ea-Type", "Subscriber");
            client.DefaultRequestHeaders.Add("X-Secret-Token", "INVALID_TOKEN");
            client.DefaultRequestHeaders.Add("X-Ea-Version", VALID_VERSION);

            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                var id = await mediator.Send(new SecretKeyCreateCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
            }

            var res = await client.PostAsync("/api/ea/token/release", new StringContent("", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidToken", await res.Content.ReadAsStringAsync());
        }
    }
}
