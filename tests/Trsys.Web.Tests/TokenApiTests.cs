using MediatR;
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
using Trsys.Web.Data;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class TokenApiTests
    {
        private const string VALID_KEY = "VALID_KEY";
        private const string VALID_VERSION = "20210331";

        [TestMethod]
        public async Task PostApiToken_should_return_ok_given_valid_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);

            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
                await mediator.Send(new GenerateSecretTokenCommand(id));
            }
            var res = await client.PostAsync("/api/token", new StringContent(VALID_KEY, Encoding.UTF8, "text/plain"));
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
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);

            var res = await client.PostAsync("/api/token", new StringContent("INVALID_SECRET_KEY", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidSecretKey", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiToken_should_return_badrequest_given_invalid_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);

            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null));
            }

            var res = await client.PostAsync("/api/token", new StringContent(VALID_KEY, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("InvalidSecretKey", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiToken_should_return_badrequest_given_in_use_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);

            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
                var token = await mediator.Send(new GenerateSecretTokenCommand(id));
                await mediator.Send(new ConnectSecretKeyCommand(id, token));
            }

            var res = await client.PostAsync("/api/token", new StringContent(VALID_KEY, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            Assert.AreEqual("SecretKeyInUse", await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiTokenRelease_should_return_ok_given_valid_token_and_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);

            var token = default(string);
            using (var scope = server.Services.CreateScope())
            {
                var mediator = server.Services.GetRequiredService<IMediator>();
                var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Subscriber, VALID_KEY, null, true));
                token = await mediator.Send(new GenerateSecretTokenCommand(id));
                await mediator.Send(new ConnectSecretKeyCommand(id, token));
            }

            var res = await client.PostAsync("/api/token/" + token + "/release", new StringContent("", Encoding.UTF8, "text/plain"));
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
            var server = CreateTestServer();
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Version", VALID_VERSION);

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
