using System.Threading.Tasks;
using Trsys.Web.Models.Events;

namespace Trsys.Web.Infrastructure.EventProcessing
{
    public class EventSubmitter : IEventSubmitter
    {
        private readonly EventQueue queue;

        public EventSubmitter(EventQueue queue)
        {
            this.queue = queue;
        }
        public Task SendAsync(Event ev)
        {
            return queue.EnqueueAsync(ev);
        }
    }
}
