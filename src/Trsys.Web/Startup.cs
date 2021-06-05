using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Trsys.Web.Configurations;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models;

namespace Trsys.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
            {
                options.InputFormatters.Add(new TextPlainInputFormatter());
            })
                .AddRazorRuntimeCompilation()
                .AddSessionStateTempDataProvider();

            services.AddSession();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.ReturnUrlParameter = "returnUrl";
                });

            services.AddMediatR(typeof(Startup).Assembly);

            services.AddSingleton(new PasswordHasher(Configuration.GetValue<string>("Trsys.Web:PasswordSalt")));
            var sqlserverConnection = Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(sqlserverConnection))
            {
                services.AddInMemoryInfrastructure();
            }
            else
            {
                var redisConnection = Configuration.GetConnectionString("RedisConnection");
                if (!string.IsNullOrEmpty(redisConnection))
                {
                    var redis = ConnectionMultiplexer.Connect(redisConnection);
                    services.AddDataProtection()
                        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
                        .SetApplicationName("Trsys.Web");
                }
                services.AddSqlServerInfrastructure(sqlserverConnection, redisConnection);
                services.AddDbContext<TrsysContext>(options => options.UseSqlServer(sqlserverConnection));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var sqlserverConnection = Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(sqlserverConnection))
            {
                logger.LogInformation("Using in-memory implementation.");
            }
            else
            {
                logger.LogInformation("Using sql server connection.");
                logger.LogInformation("Database initializing.");
                DatabaseInitializer.InitializeAsync(app).Wait();
                logger.LogInformation("Database initialized.");
            }
            DatabaseInitializer.SeedDataAsync(app).Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
