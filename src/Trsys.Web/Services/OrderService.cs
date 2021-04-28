using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.Orders;
using Trsys.Web.Services.Events;

namespace Trsys.Web.Services
{
    public class OrderService
    {
        private readonly IMediator mediator;
        private readonly IOrderRepository repository;

        public OrderService(IOrderRepository repository, IMediator mediator)
        {
            this.repository = repository;
            this.mediator = mediator;
        }

        public async Task UpdateOrdersAsync(IEnumerable<Order> orders)
        {
            await repository.SaveOrdersAsync(orders);
            await mediator.Publish(new OrderUpdated(orders));
        }

        public Task ClearOrdersAsync()
        {
            return UpdateOrdersAsync(new List<Order>());
        }
    }
}
