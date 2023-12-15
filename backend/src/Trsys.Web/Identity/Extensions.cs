using Microsoft.AspNetCore.Identity;

namespace Trsys.Web.Identity
{
    public static class Extensions
    {
        public static IdentityBuilder AddIdentityStore(this IdentityBuilder builder)
        {
            builder.AddUserStore<IdentityUserStore>();
            builder.AddRoleStore<IdentityRoleStore>();
            return builder;
        }
    }
}