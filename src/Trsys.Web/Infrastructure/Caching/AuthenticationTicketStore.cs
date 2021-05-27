using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using Trsys.Web.Authentication;

namespace Trsys.Web.Infrastructure.Caching
{
    public class AuthenticationTicketStore : IAuthenticationTicketStore
    {
        private readonly IKeyValueStore<byte[]> store;

        public AuthenticationTicketStore(IKeyValueStoreFactory factory)
        {
            this.store = factory.Create<byte[]>("AuthenticationTicket");
        }

        public Task AddAsync(string token, AuthenticationTicket ticket)
        {
            return store.PutAsync(token, TicketSerializer.Default.Serialize(ticket));
        }

        public async Task<AuthenticationTicket> FindAsync(string token)
        {
            var data = await store.GetAsync(token);
            if (data == null)
            {
                return null;
            }
            return TicketSerializer.Default.Deserialize(data);
        }

        public async Task<AuthenticationTicket> RemoveAsync(string token)
        {
            var ticket = await FindAsync(token);
            await store.DeleteAsync(token);
            return ticket;
        }
    }
}
