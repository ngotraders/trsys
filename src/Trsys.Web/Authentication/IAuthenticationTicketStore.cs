using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Trsys.Web.Authentication
{
    public interface IAuthenticationTicketStore
    {
        void Add(string token, ClaimsPrincipal principal);
        AuthenticationTicket Find(string token);
        AuthenticationTicket Remove(string token);
    }
}
