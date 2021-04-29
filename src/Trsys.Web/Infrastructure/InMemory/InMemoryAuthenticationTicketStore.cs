using Microsoft.AspNetCore.Authentication;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Trsys.Web.Authentication;

namespace Trsys.Web.Infrastructure.InMemory
{
    public class InMemoryAuthenticationTicketStore : IAuthenticationTicketStore
    {
        private readonly ConcurrentDictionary<string, AuthenticationTicket> store = new ConcurrentDictionary<string, AuthenticationTicket>();

        public Task AddAsync(string token, AuthenticationTicket ticket)
        {
            store.TryAdd(token, ticket);
            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket> FindAsync(string token)
        {
            store.TryGetValue(token, out var value);
            return Task.FromResult(value);
        }

        public Task<AuthenticationTicket> RemoveAsync(string token)
        {
            store.TryRemove(token, out var value);
            return Task.FromResult(value);
        }
    }
}
