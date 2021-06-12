using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Models.Messaging
{
    public class PublishingMessageHandler : INotificationHandler<PublishingMessageEnvelope>
    {
        private readonly IPublishingMessageProcessor processor;

        public PublishingMessageHandler(IPublishingMessageProcessor processor)
        {
            this.processor = processor;
        }

        public Task Handle(PublishingMessageEnvelope notification, CancellationToken cancellationToken)
        {
            return processor.Enqueue(notification, cancellationToken);
        }
    }
}
