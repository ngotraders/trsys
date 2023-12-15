using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Trsys.Infrastructure;
using Trsys.Infrastructure.ReadModel.UserNotification;
using Trsys.Models;
using Trsys.Web.Configurations;
using Trsys.Web.Identity;
using Trsys.Web.Middlewares;

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

            services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();
            services.AddAuthorizationBuilder();
            services.AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddIdentityStore()
                .AddApiEndpoints();

            services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Startup).Assembly));

            var sqliteConnection = Configuration.GetConnectionString("SQLiteConnection");
            var sqlserverConnection = string.IsNullOrEmpty(sqliteConnection) ? Configuration.GetConnectionString("DefaultConnection") : null;
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
            services.AddEmailSender(Configuration.GetSection("Trsys.Web:EmailSenderConfiguration").Get<EmailSenderConfiguration>());
            if (!string.IsNullOrEmpty(sqliteConnection))
            {
                services.AddDbContext<TrsysContext>(options => options.UseSqlite(sqliteConnection));
            }
            else
            {
                services.AddDbContext<TrsysContext>(options => options.UseSqlServer(sqlserverConnection));
                services.AddDataProtection().PersistKeysToDbContext<TrsysContext>();
            }
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var task = Task.Run(async () =>
            {
                logger.LogInformation("Database initializing.");
                await DatabaseInitializer.InitializeAsync(app);
                logger.LogInformation("Database initialized.");
            });
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
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(config =>
            {
                config.AllowCredentials();
                config.AllowAnyHeader();
                config.AllowAnyMethod();
                config.SetIsOriginAllowed(origin => true);
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseInitialization(task);
            app.UseSession();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapIdentityApi<IdentityUser>();
                endpoints.MapControllers();
            });
        }
    }
}
