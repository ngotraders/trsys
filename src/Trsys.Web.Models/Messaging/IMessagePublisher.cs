using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Models.Messaging
{
    public interface IMessagePublisher
    {
        Task Enqueue(PublishingMessageEnvelope notification, CancellationToken cancellationToken);
    }
}
