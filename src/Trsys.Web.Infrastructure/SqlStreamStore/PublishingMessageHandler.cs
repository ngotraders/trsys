using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public class PublishingMessageHandler : INotificationHandler<PublishingMessageEnvelope>
    {
        private readonly PublishingMessageProcessor processor;

        public PublishingMessageHandler(PublishingMessageProcessor processor)
        {
            this.processor = processor;
        }

        public Task Handle(PublishingMessageEnvelope notification, CancellationToken cancellationToken)
        {
            return processor.Enqueue(notification, cancellationToken);
        }
    }
}
