using MediatR;
using SqlStreamStore.Infrastructure;
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
        INotificationHandler<SecretKeyDeleted>,
        IRequestHandler<GetOrderTextEntry, OrdersTextEntry>,
        IRequestHandler<GetOrders, List<OrderDto>>,
        IRequestHandler<GetPublishedOrders, List<PublishedOrder>>
    {
        private static readonly TaskQueue quque = new();
        private readonly OrderInMemoryDatabase db;

        public OrderQueryHandler(OrderInMemoryDatabase db)
        {
            this.db = db;
        }

        public Task Handle(OrderPublisherOpenedOrder notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.Add(new OrderDto()
                {
                    Id = $"{notification.Id}:{notification.Order.TicketNo}",
                    Order = notification.Order,
                    SecretKeyId = notification.Id,
                });
            });
        }

        public Task Handle(OrderPublisherClosedOrder notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.Remove($"{notification.Id}:{notification.TicketNo}");
            });
        }

        public Task Handle(SecretKeyDeleted notification, CancellationToken cancellationToken)
        {
            return quque.Enqueue(() =>
            {
                db.RemoveBySecretKey(notification.Id);
            });
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
