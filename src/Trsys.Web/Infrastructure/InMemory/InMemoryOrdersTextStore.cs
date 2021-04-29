using System.Threading.Tasks;
using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure.InMemory
{
    public class InMemoryOrdersTextStore : IOrdersTextStore
    {
        private OrdersTextEntry _entry;

        public InMemoryOrdersTextStore()
        {
        }

        public Task<OrdersTextEntry> GetOrdersTextAsync()
        {
            return Task.FromResult(_entry);
        }

        public Task UpdateOrdersTextAsync(OrdersTextEntry textEntry)
        {
            _entry = textEntry;
            return Task.CompletedTask;
        }
    }
}
