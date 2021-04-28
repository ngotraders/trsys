using Microsoft.Extensions.Caching.Memory;
using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure
{
    public class OrdersCacheManager : IOrdersTextStore
    {
        private readonly IMemoryCache cache;

        public OrdersCacheManager(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public bool TryGetOrdersText(out OrdersTextEntry textEntry)
        {
            return cache.TryGetValue(CacheKeys.ORDERS_CACHE, out textEntry);
        }

        public void UpdateOrdersText(OrdersTextEntry textEntry)
        {
            cache.Set(CacheKeys.ORDERS_CACHE, textEntry);
        }
    }
}
