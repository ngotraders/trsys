using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Models.Messaging
{
    public interface IMessagePublisher
    {
        Task Enqueue(PublishingMessageEnvelope notification, CancellationToken cancellationToken);
    }
}
