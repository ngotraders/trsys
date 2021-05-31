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
using Trsys.Web.Data;
using Trsys.Web.Infrastructure;
using Trsys.Web.Services;

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

            services.AddInMemoryInfrastructure();

            services.AddSingleton(new PasswordHasher(Configuration.GetValue<string>("Trsys.Web:PasswordSalt")));
            var sqliteConnection = Configuration.GetConnectionString("SqliteConnection");
            if (string.IsNullOrEmpty(sqliteConnection))
            {
                services.AddDbContext<TrsysContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
                services.AddRepositories();
            }
            else
            {
                services.AddDbContext<TrsysContext>(options => options.UseSqlite(sqliteConnection));
                services.AddSQLiteRepositories();
            }
            var redisConnection = Configuration.GetConnectionString("RedisConnection");
            if (string.IsNullOrEmpty(redisConnection))
            {
                services.AddInMemoryStores();
            }
            else
            {
                var redis = ConnectionMultiplexer.Connect(redisConnection);
                services.AddDataProtection()
                    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
                    .SetApplicationName("Trsys.Web");
                services.AddRedisStores(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "Trsys.Web/";
                });
            }
            services.AddEventProcessor();
            services.AddTransient<OrderService>();
            services.AddTransient<UserService>();
            services.AddTransient<EventService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var sqliteConnection = Configuration.GetConnectionString("SqliteConnection");
            if (string.IsNullOrEmpty(sqliteConnection))
            {
                logger.LogInformation("Using sql server connection.");
            }
            else
            {
                logger.LogInformation("Using sqlite connection.");
            }
            var redisConnection = Configuration.GetConnectionString("RedisConnection");
            if (string.IsNullOrEmpty(redisConnection))
            {
                logger.LogInformation("Using in memory implementation for key-value stores.");
            }
            else
            {
                logger.LogInformation("Using redis implementation for key-value stores.");
            }
            logger.LogInformation("Database initializing.");
            DatabaseInitializer.InitializeAsync(app).Wait();
            logger.LogInformation("Database initialized.");

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
