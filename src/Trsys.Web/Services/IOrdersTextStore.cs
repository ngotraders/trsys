using System.Threading.Tasks;

namespace Trsys.Web.Services
{
    public interface IOrdersTextStore
    {
        Task UpdateOrdersTextAsync(OrdersTextEntry textEntry);
        Task<OrdersTextEntry> GetOrdersTextAsync();
    }
}
