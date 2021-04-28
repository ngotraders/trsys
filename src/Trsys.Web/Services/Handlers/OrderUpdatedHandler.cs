using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Caching;
using Trsys.Web.Services.Events;

namespace Trsys.Web.Services.Handlers
{
    public class OrderUpdatedHandler : INotificationHandler<OrderUpdated>
    {
        private readonly OrdersCacheManager cache;

        public OrderUpdatedHandler(OrdersCacheManager cache)
        {
            this.cache = cache;
        }

        public Task Handle(OrderUpdated notification, CancellationToken cancellationToken)
        {
            cache.UpdateOrdersCache(notification.Orders.ToList());
            return Task.CompletedTask;
        }
    }
}
