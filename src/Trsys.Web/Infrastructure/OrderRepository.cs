using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Infrastructure
{
    public class OrderRepository : IOrderRepository
    {
        private readonly TrsysContextProcessor processor;

        public OrderRepository(TrsysContextProcessor processor)
        {
            this.processor = processor;
        }

        public Task SaveOrdersAsync(IEnumerable<Order> orders)
        {
            return processor.Enqueue(db =>
            {
                db.Orders.RemoveRange(db.Orders);
                db.Orders.AddRange(orders);
                return db.SaveChangesAsync();
            });
        }

        public Task<List<Order>> SearchAllAsync()
        {
            return processor.Enqueue(db => db.Orders.ToListAsync());
        }
    }
}
