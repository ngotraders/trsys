using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Events;
using Trsys.Web.Models.ReadModel.Infrastructure;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Models.ReadModel.Handlers
{
    public class OrderQueryHandler :
        INotificationHandler<OrderPublisherOpenedOrder>,
        INotificationHandler<OrderPublisherClosedOrder>,
        IRequestHandler<GetOrderTextEntry, OrdersTextEntry>,
        IRequestHandler<GetOrders, List<OrderDto>>,
        IRequestHandler<GetPublishedOrders, List<PublishedOrder>>
    {
        private readonly OrderInMemoryDatabase db;

        public OrderQueryHandler(OrderInMemoryDatabase db)
        {
            this.db = db;
        }

        public Task Handle(OrderPublisherOpenedOrder notification, CancellationToken cancellationToken = default)
        {
            db.Add(new OrderDto()
            {
                Id = $"{notification.Id}:{notification.Order.TicketNo}",
                Order = notification.Order
            });
            return Task.CompletedTask;
        }

        public Task Handle(OrderPublisherClosedOrder notification, CancellationToken cancellationToken = default)
        {
            db.Remove($"{notification.Id}:{notification.TicketNo}");
            return Task.CompletedTask;
        }

        public Task<OrdersTextEntry> Handle(GetOrderTextEntry request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(db.Entry);
        }

        public Task<List<OrderDto>> Handle(GetOrders request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(db.All);
        }

        public Task<List<PublishedOrder>> Handle(GetPublishedOrders request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(db.List);
        }
    }
}
