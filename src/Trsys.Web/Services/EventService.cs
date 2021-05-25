using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.Events;

namespace Trsys.Web.Services
{
    public class EventService
    {
        private readonly IEventSubmitter submitter;
        private readonly IEventRepository repository;

        public EventService(IEventSubmitter submitter, IEventRepository repository)
        {
            this.submitter = submitter;
            this.repository = repository;
        }

        public Task<List<Event>> SearchAsync(string source, int page, int perPage)
        {
            return repository.SearchAsync(source, page, perPage);
        }

        public Task RegisterSystemEventAsync(string category, string eventType, object data = null)
        {
            return submitter.SendAsync(Event.Create($"system/{category}", eventType, data));
        }

        public Task RegisterEaEventAsync(string secretKey, string eventType, object data = null)
        {
            return submitter.SendAsync(Event.Create($"ea/{secretKey}", eventType, data));
        }

        public Task RegisterUserEventAsync(string username, string eventType, object data = null)
        {
            return submitter.SendAsync(Event.Create($"user/{username}", eventType, data));
        }
    }
}
