using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.Events;

namespace Trsys.Web.Infrastructure.EventProcessing
{
    public class EventQueue
    {
        private BlockingCollection<Event> queue = new BlockingCollection<Event>();

        public Task<Event> DequeueAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => queue.Take(cancellationToken));
        }

        public async Task EnqueueAsync(Event ev)
        {
            await Task.Run(() =>
            {
                queue.Add(ev);
            });
        }
    }
}
