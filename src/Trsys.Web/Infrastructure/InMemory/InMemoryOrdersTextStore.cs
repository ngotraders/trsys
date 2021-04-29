using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure.InMemory
{
    public class InMemoryOrdersTextStore : IOrdersTextStore
    {
        private OrdersTextEntry _entry;

        public InMemoryOrdersTextStore()
        {
        }

        public bool TryGetOrdersText(out OrdersTextEntry textEntry)
        {
            textEntry = _entry;
            return textEntry == null;
        }

        public void UpdateOrdersText(OrdersTextEntry textEntry)
        {
            _entry = textEntry;
        }
    }
}
