using Microsoft.AspNetCore.Authentication;
using System.Collections.Concurrent;
using System.Security.Claims;
using Trsys.Web.Authentication;

namespace Trsys.Web.Infrastructure
{
    public class InMemoryAuthenticationTicketStore : IAuthenticationTicketStore
    {
        private readonly ConcurrentDictionary<string, AuthenticationTicket> store = new ConcurrentDictionary<string, AuthenticationTicket>();

        public void Add(string token, ClaimsPrincipal principal)
        {
            var ticket = new AuthenticationTicket(principal, "SecretKey");
            store.TryAdd(token, ticket);
        }

        public AuthenticationTicket Find(string token)
        {
            store.TryGetValue(token, out var value);
            return value;
        }

        public AuthenticationTicket Remove(string token)
        {
            store.TryRemove(token, out var value);
            return value;
        }
    }
}
