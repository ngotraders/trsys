using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Caching;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Services
{
    public class OrderService
    {
        private readonly OrdersCacheManager cache;
        private readonly IOrderRepository repository;

        public OrderService(IOrderRepository repository, OrdersCacheManager cache)
        {
            this.repository = repository;
            this.cache = cache;
        }

        public async Task UpdateOrdersAsync(IEnumerable<Order> orders)
        {
            await repository.SaveOrdersAsync(orders);
            cache.UpdateOrdersCache(orders.ToList());
        }

        public Task ClearOrdersAsync()
        {
            return UpdateOrdersAsync(new List<Order>());
        }

        public OrdersCache GetOrderTextEntry()
        {
            cache.TryGetCache(out var cacheEntry);
            return cacheEntry;
        }
    }
}
