using Microsoft.AspNetCore.Authentication;

namespace Trsys.Web.Authentication
{
    public class SecretTokenAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public IAuthenticationTicketStore Store { get; set; }
    }
}
