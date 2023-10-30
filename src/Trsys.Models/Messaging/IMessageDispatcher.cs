using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Models.Messaging
{
    public interface IMessageDispatcher
    {
        Task DispatchAsync(PublishingMessage message, CancellationToken cancellationToken = default);
    }
}