using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Services
{
    public class OrderService
    {
        private readonly IOrdersTextStore orderTextStore;
        private readonly IOrderRepository repository;

        public OrderService(IOrderRepository repository, IOrdersTextStore orderTextStore)
        {
            this.repository = repository;
            this.orderTextStore = orderTextStore;
        }

        public async Task UpdateOrdersAsync(IEnumerable<Order> orders)
        {
            await repository.SaveOrdersAsync(orders);
            orderTextStore.UpdateOrdersText(OrdersTextEntry.Create(orders.ToList()));
        }

        public Task ClearOrdersAsync()
        {
            return UpdateOrdersAsync(new List<Order>());
        }

        public OrdersTextEntry GetOrderTextEntry()
        {
            orderTextStore.TryGetOrdersText(out var cacheEntry);
            return cacheEntry;
        }
    }
}
