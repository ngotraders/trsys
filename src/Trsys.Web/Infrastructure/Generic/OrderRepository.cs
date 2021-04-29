using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Infrastructure.Generic
{
    public class OrderRepository : IOrderRepository
    {
        private readonly TrsysContext db;

        public OrderRepository(TrsysContext db)
        {
            this.db = db;
        }

        public Task SaveOrdersAsync(IEnumerable<Order> orders)
        {
            db.Orders.RemoveRange(db.Orders);
            db.Orders.AddRange(orders);
            return db.SaveChangesAsync();
        }

        public Task<List<Order>> SearchAllAsync()
        {
            return db.Orders.ToListAsync();
        }
    }
}
