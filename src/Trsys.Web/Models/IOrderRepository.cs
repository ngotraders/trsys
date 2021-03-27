using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trsys.Web.Models
{
    public interface IOrderRepository
    {
        IQueryable<Order> AllOrders { get; }
        Task SaveOrdersAsync(IEnumerable<Order> orders);
    }
}
