using System.Collections.Generic;
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
}