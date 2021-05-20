using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Linq;
using Trsys.Web.Authentication;
using Trsys.Web.Configurations;
using Trsys.Web.Data;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models.Users;
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
                })
                .AddSecretTokenAuthentication();

            services.AddSingleton(new PasswordHasher(Configuration.GetValue<string>("Trsys.Web:PasswordSalt")));
            var sqliteConnection = Configuration.GetConnectionString("SqliteConnection");
            if (string.IsNullOrEmpty(sqliteConnection))
            {
                services.AddDbContext<TrsysContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
                services.AddSQLiteRepositories();
            }
            else
            {
                services.AddDbContext<TrsysContext>(options => options.UseSqlite(sqliteConnection));
                services.AddRepositories();
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
                    options.InstanceName = "Trsys.Web";
                });
            }
            services.AddEventProcessor();
            services.AddTransient<OrderService>();
            services.AddTransient<SecretKeyService>();
            services.AddTransient<UserService>();
            services.AddTransient<EventService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<TrsysContext>())
            {
                db.Database.Migrate();
                if (!db.Users.Any())
                {
                    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
                    db.Users.Add(new User()
                    {
                        Username = "admin",
                        Password = passwordHasher.Hash("P@ssw0rd"),
                        Role = "Administrator",
                    });
                    db.SaveChanges();
                }

                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                orderService.RefreshOrderTextAsync().Wait();
            }

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
