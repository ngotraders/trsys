using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using Trsys.Web.Authentication;

namespace Trsys.Web.Infrastructure.Redis
{
    public class RedisAuthenticationTicketStore : IAuthenticationTicketStore
    {
        private readonly IDistributedCache cache;

        public RedisAuthenticationTicketStore(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public async Task AddAsync(string token, AuthenticationTicket ticket)
        {
            await cache.SetAsync("AuthTicket:" + token, TicketSerializer.Default.Serialize(ticket));
        }

        public async Task<AuthenticationTicket> FindAsync(string token)
        {
            return TicketSerializer.Default.Deserialize(await cache.GetAsync("AuthTicket:" + token));
        }

        public async Task<AuthenticationTicket> RemoveAsync(string token)
        {
            var ticket = await FindAsync(token);
            await cache.RemoveAsync("AuthTicket:" + token);
            return ticket;
        }
    }
}
