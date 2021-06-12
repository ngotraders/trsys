using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace Trsys.Web.Models.Messaging
{
    public class PublishingMessageEnvelope : INotification
    {
        private PublishingMessageEnvelope(IEnumerable<PublishingMessage> messages)
        {
            Payload = messages.ToList();
        }

        public List<PublishingMessage> Payload { get; set; }

        public static PublishingMessageEnvelope Create(INotification notification)
        {
            return new PublishingMessageEnvelope(new[] { MessageConverter.ConvertFromNotification(notification) });
        }

        public static PublishingMessageEnvelope Create(IEnumerable<PublishingMessage> messages)
        {
            return new PublishingMessageEnvelope(messages);
        }
    }
}
