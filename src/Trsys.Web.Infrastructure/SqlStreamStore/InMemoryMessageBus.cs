using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public class InMemoryMessageBus : IMessageBus
    {
        private readonly IMediator mediator;

        public InMemoryMessageBus(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public Task Publish(PublishedMessage message, CancellationToken cancellationToken = default)
        {
            return Publish(MessageConverter.ConvertToNotification(message), cancellationToken);
        }

        public Task Publish(INotification notification, CancellationToken cancellationToken = default)
        {
            return mediator.Publish(notification, cancellationToken);
        }
    }
}
