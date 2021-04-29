using Microsoft.AspNetCore.Authentication;
using Trsys.Web.Services;

namespace Trsys.Web.Authentication
{
    public class SecretTokenAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public IAuthenticationTicketStore Store { get; set; }
        public SecretKeyService Service { get; set; }
    }
}
