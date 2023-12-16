using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Trsys.Infrastructure;
using Trsys.Web.Identity;
using Trsys.Web.Middlewares;
using Trsys.Web.Models;

namespace Trsys.Web.Tests
{
    public static class TestHelper
    {
        public static Task<IHost> CreateTestServerAsync()
        {
            var assembly = Assembly.Load("Trsys.Web");
            return new HostBuilder()
                .UseSerilog(new LoggerConfiguration().WriteTo.Console().CreateLogger())
                .ConfigureServices(services =>
                {
                    services.AddEndpointsApiExplorer();
                    services.AddTrsysIdentity();
                    services.AddControllers()
                        .PartManager.ApplicationParts.Add(new AssemblyPart(assembly)); ;

                    services.AddEndpointsApiExplorer();
                    services.AddSwaggerGen();

                    services.AddMediatR(options => options.RegisterServicesFromAssembly(assembly));
                    services.AddInMemoryInfrastructure();
                    services.AddEmailSender();
                    services.AddDbContext<TrsysContext>(options => options.UseInMemoryDatabase("Trsys"));
                })
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    webBuilder.Configure(app =>
                    {
                        app.UseHttpsRedirection();
                        app.UseInitialization();
                        app.UseRouting();
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoint =>
                        {
                            endpoint.MapIdentityApi<TrsysUser>();
                            endpoint.MapControllers();
                        });
                    });
                })
                .StartAsync();
        }
    }

    public static class HttpClientExtension
    {
        public static async Task LoginAsync(this HttpClient client)
        {
            var loginResponse = await client.PostAsync("/login?useCookie=true", JsonContent.Create(new
            {
                email = "admin@example.com",
                password = "P@ssw0rd",
            }));

            var container = new CookieContainer();
            container.SetCookies(client.BaseAddress, loginResponse.Headers.GetValues("Set-Cookie").FirstOrDefault());
            client.DefaultRequestHeaders.Add("Cookie", container.GetCookieHeader(client.BaseAddress));
        }
    }
}