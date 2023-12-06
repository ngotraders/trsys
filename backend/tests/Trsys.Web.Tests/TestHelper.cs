using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Trsys.Web.Tests
{
    public static class TestHelper
    {
        public static TestServer CreateServer()
        {
            return new TestServer(new WebHostBuilder()
                .UseSerilog(new LoggerConfiguration().WriteTo.Console().CreateLogger())
                .UseConfiguration(
                    new ConfigurationBuilder()
                    .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Trsys.Web:PasswordSalt", "salt"), }).Build()
                 )
                .UseStartup<Startup>());
        }
    }

    public static class HttpClientExtension
    {
        public static async Task LoginAsync(this HttpClient client)
        {
            var loginResponse = await client.PostAsync("/login", new FormUrlEncodedContent(
                new KeyValuePair<string, string>[] {
                        KeyValuePair.Create("Username", "admin"),
                        KeyValuePair.Create("Password", "P@ssw0rd"),
                }));

            var container = new CookieContainer();
            container.SetCookies(client.BaseAddress, loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault());
            client.DefaultRequestHeaders.Add("Cookie", container.GetCookieHeader(client.BaseAddress));
        }
    }
}