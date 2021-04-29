using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure.Redis
{
    public class RedisOrdersTextStore : IOrdersTextStore
    {
        private readonly IDistributedCache cache;

        public RedisOrdersTextStore(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public Task<OrdersTextEntry> GetOrdersTextAsync()
        {
            return cache.GetObjectAsync<OrdersTextEntry>("OrdersText");
        }

        public Task UpdateOrdersTextAsync(OrdersTextEntry textEntry)
        {
            return cache.SetObjectAsync("OrdersText", textEntry);
        }
    }
}
