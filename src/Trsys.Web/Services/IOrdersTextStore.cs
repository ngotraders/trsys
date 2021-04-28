namespace Trsys.Web.Services
{
    public interface IOrdersTextStore
    {
        void UpdateOrdersText(OrdersTextEntry textEntry);
        bool TryGetOrdersText(out OrdersTextEntry textEntry);
    }
}
