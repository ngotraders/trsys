using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public class MessagePublisher : INotificationHandler<PublishingMessageEnvelope>
    {
        private readonly IMediator mediator;

        public MessagePublisher(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task Handle(PublishingMessageEnvelope notification, CancellationToken cancellationToken)
        {
            foreach (var n in notification.Payload)
            {
                await mediator.Publish(MessageConverter.ConvertToNotification(n), cancellationToken);
            }
        }
    }
}
