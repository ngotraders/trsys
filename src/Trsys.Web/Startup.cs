using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Trsys.Web.Configurations;
using Trsys.Infrastructure;
using Trsys.Web.Middlewares;
using Trsys.Models;
using Trsys.Infrastructure.ReadModel.UserNotification;

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

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                // You might want to only set the application cookies over a secure connection:
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.ReturnUrlParameter = "returnUrl";
                });

            services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Startup).Assembly));

            services.AddSingleton(new PasswordHasher(Configuration.GetValue<string>("Trsys.Web:PasswordSalt")));
            var sqlserverConnection = Configuration.GetConnectionString("DefaultConnection");
            var redisConnection = Configuration.GetConnectionString("RedisConnection");
            var blobStorageConnection = Configuration.GetConnectionString("BlobStorageConnection");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                //Add distributed cache service backed by Redis cache
                services.AddStackExchangeRedisCache(o =>
                {
                    o.Configuration = redisConnection;
                });
            }
            services.AddInfrastructure(sqlserverConnection, redisConnection);
            services.AddEmailSender();
            services.AddDbContext<TrsysContext>(options => options.UseSqlServer(sqlserverConnection));
            if (!string.IsNullOrEmpty(sqlserverConnection))
            {
                services.AddDataProtection().PersistKeysToDbContext<TrsysContext>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var sqlserverConnection = Configuration.GetConnectionString("DefaultConnection");
            var task = Task.CompletedTask;
            if (string.IsNullOrEmpty(sqlserverConnection))
            {
                logger.LogInformation("Using in-memory implementation for database.");
            }
            else
            {
                logger.LogInformation("Using sql server connection.");
                task = Task.Run(async () =>
                {
                    logger.LogInformation("Database initializing.");
                    await DatabaseInitializer.InitializeAsync(app);
                    logger.LogInformation("Database initialized.");
                });
            }
            var redisConnection = Configuration.GetConnectionString("RedisConnection");
            if (string.IsNullOrEmpty(redisConnection))
            {
                logger.LogInformation("Using in-memory implementation for redis.");
            }
            else
            {
                logger.LogInformation("Using redis implementation.");
            }
            task = task.ContinueWith(task => DatabaseInitializer.SeedDataAsync(app));

            // 最大で1秒待つ
            Task.WhenAny(Task.Delay(1000), task).Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseInitialization(task);
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
