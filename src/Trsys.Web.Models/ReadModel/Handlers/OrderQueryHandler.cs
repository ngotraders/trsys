using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Infrastructure;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Models.ReadModel.Handlers
{
    public class OrderQueryHandler :
        INotificationHandler<OrderPublisherOpenedOrder>,
        INotificationHandler<OrderPublisherClosedOrder>,
        INotificationHandler<SecretKeyDeleted>,
        IRequestHandler<GetOrderTextEntry, OrdersTextEntry>,
        IRequestHandler<GetOrders, List<OrderDto>>,
        IRequestHandler<GetPublishedOrders, List<PublishedOrder>>
    {
        private readonly IOrderDatabase db;

        public OrderQueryHandler(IOrderDatabase db)
        {
            this.db = db;
        }

        public Task Handle(OrderPublisherOpenedOrder notification, CancellationToken cancellationToken = default)
        {
            return db.AddAsync(new OrderDto()
            {
                Id = $"{notification.Id}:{notification.Order.TicketNo}",
                SecretKeyId = notification.Id,
                TicketNo = notification.Order.TicketNo,
                Order = notification.Order,
            });
        }

        public Task Handle(OrderPublisherClosedOrder notification, CancellationToken cancellationToken = default)
        {
            return db.RemoveAsync($"{notification.Id}:{notification.TicketNo}");
        }

        public Task Handle(SecretKeyDeleted notification, CancellationToken cancellationToken)
        {
            return db.RemoveBySecretKeyAsync(notification.Id);
        }

        public Task<OrdersTextEntry> Handle(GetOrderTextEntry request, CancellationToken cancellationToken = default)
        {
            return db.FindEntryAsync(request.Version);
        }

        public Task<List<OrderDto>> Handle(GetOrders request, CancellationToken cancellationToken = default)
        {
            return db.SearchAsync();
        }

        public Task<List<PublishedOrder>> Handle(GetPublishedOrders request, CancellationToken cancellationToken = default)
        {
            return db.SearchPublishedOrderAsync();
        }
    }
}
