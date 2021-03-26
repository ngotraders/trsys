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
using Trsys.Web.Models;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class TokenApiTests
    {
        private const string VALID_SECRET_KEY = "VALID_SECRET_KEY";
        private const string SECRET_KEY_IN_USE = "SECRET_KEY_IN_USE";
        private const string VALID_TOKEN = "VALID_TOKEN";

        [TestMethod]
        public async Task PostApiToken_should_return_ok_given_valid_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            var res = await client.PostAsync("/api/token", new StringContent(VALID_SECRET_KEY, Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(VALID_TOKEN, await res.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task PostApiToken_should_return_badrequest_given_invalid_secret_key()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            var res = await client.PostAsync("/api/token", new StringContent("INVALID_SECRET_KEY", Encoding.UTF8, "text/plain"));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
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
                                services.AddSingleton<ITokenGenerator, MockTokenGenerator>();
                            }));
        }

        private class MockTokenGenerator : ITokenGenerator
        {
            public Task<TokenGenerationResult> GenerateTokenAsync(string secretKey)
            {
                var res = null as TokenGenerationResult;
                if (secretKey == VALID_SECRET_KEY)
                {
                    res = TokenGenerationResult.Success(VALID_TOKEN);
                }
                else if (secretKey == SECRET_KEY_IN_USE)
                {
                    res = TokenGenerationResult.Fail(TokenGenerationFailureReason.TokenInUse);
                }
                else
                {
                    res = TokenGenerationResult.Fail(TokenGenerationFailureReason.InvalidToken);
                }
                return Task.FromResult(res);
            }
        }
    }
}
