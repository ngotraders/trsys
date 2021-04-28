using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Trsys.Web.Authentication;
using Trsys.Web.Caching;
using Trsys.Web.Configurations;
using Trsys.Web.Data;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models.Orders;
using Trsys.Web.Models.SecretKeys;
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
                })
                .AddSecretTokenAuthentication();

            services.AddMemoryCache();
            services.AddMediatR(options => options.AsSingleton(), typeof(Startup));
            services.AddSingleton(new TrsysContext(new DbContextOptionsBuilder<TrsysContext>()
                .UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
                .Options));
            services.AddSingleton<TrsysContextProcessor>();
            services.AddSingleton<OrdersCacheManager>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ISecretKeyRepository, SecretKeyRepository>();
            services.AddSingleton<ISecretTokenStore, InMemorySecretTokenStore>();
            services.AddSingleton(new PasswordHasher(Configuration.GetValue<string>("Trsys.Web:PasswordSalt")));
            services.AddTransient<OrderService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TrsysContext>();
                {
                    db.Database.EnsureCreated();
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

                    var cacheManager = scope.ServiceProvider.GetRequiredService<OrdersCacheManager>();
                    cacheManager.UpdateOrdersCache(db.Orders.ToList());
                }
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
