using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace Trsys.Web.Authentication
{
    public interface IAuthenticationTicketStore
    {
        Task AddAsync(string token, AuthenticationTicket ticket);
        Task<AuthenticationTicket> FindAsync(string token);
        Task<AuthenticationTicket> RemoveAsync(string token);
    }
}
