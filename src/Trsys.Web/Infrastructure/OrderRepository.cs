using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Models;

namespace Trsys.Web.Infrastructure
{
    public class OrderRepository : IOrderRepository
    {
        private readonly TrsysContext db;

        public OrderRepository(TrsysContext db)
        {
            this.db = db;
        }

        public IQueryable<Order> All => db.Orders;

        public Task SaveOrdersAsync(IEnumerable<Order> orders)
        {
            db.Orders.RemoveRange(db.Orders);
            db.Orders.AddRange(orders);
            return db.SaveChangesAsync();
        }
    }
}
