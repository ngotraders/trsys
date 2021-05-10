using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Generic;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Infrastructure.SQLite
{
    public class SQLiteOrderRepository : IOrderRepository
    {
        private readonly TrsysContextProcessor processor;

        public SQLiteOrderRepository(TrsysContextProcessor processor)
        {
            this.processor = processor;
        }

        public Task<List<Order>> SearchAllAsync()
        {
            return processor.Enqueue(db => new OrderRepository(db).SearchAllAsync());
        }

        public Task SaveOrdersAsync(IEnumerable<Order> orders)
        {
            return processor.Enqueue(db => new OrderRepository(db).SaveOrdersAsync(orders));
        }
    }
}
