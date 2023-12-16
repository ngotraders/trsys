using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Trsys.Web.Identity;

public static class Extensions
{
    public static IServiceCollection AddTrsysIdentity(this IServiceCollection services)
    {
        services.AddIdentity<TrsysUser, TrsysRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddUserStore<TrsysUserStore>()
            .AddRoleStore<TrsysRoleStore>()
            .AddApiEndpoints();
        services.AddAuthentication()
            .AddCookie(IdentityConstants.BearerScheme);
        services.AddTransient<IEmailSender<TrsysUser>, TrsysIdentityEmailSender>();
        services.AddAuthorizationBuilder()
            .AddPolicy("Administrator", policy => policy.RequireRole("Administrator"))
            .AddPolicy("User", policy => policy.RequireRole("User"));

        return services;
    }
}