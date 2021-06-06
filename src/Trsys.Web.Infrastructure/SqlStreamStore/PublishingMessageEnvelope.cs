using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public class PublishingMessageEnvelope : INotification
    {
        public PublishingMessageEnvelope(IEnumerable<PublishingMessage> messages)
        {
            Payload = messages.ToList();
        }

        public List<PublishingMessage> Payload { get; set; }
    }
}
