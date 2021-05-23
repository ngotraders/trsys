using System.Threading.Tasks;
using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure.KeyValueStores
{
    public class OrdersTextStore : IOrdersTextStore
    {
        private readonly IKeyValueStore<OrdersTextEntry> store;

        public OrdersTextStore(IKeyValueStoreFactory factory)
        {
            this.store = factory.Create<OrdersTextEntry>();
        }

        public Task<OrdersTextEntry> GetOrdersTextAsync()
        {
            return store.GetAsync("OrdersText");
        }

        public Task UpdateOrdersTextAsync(OrdersTextEntry textEntry)
        {
            return store.PutAsync("OrdersText", textEntry);
        }
    }
}
