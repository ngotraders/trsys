using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public interface IMessageBus
    {
        Task Publish(PublishedMessage message, CancellationToken cancellationToken = default);
        Task Publish(INotification notification, CancellationToken cancellationToken = default);
    }
}
