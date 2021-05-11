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

        public Task<List<Event>> SearchAsync(string key, int page, int perPage)
        {
            return repository.SearchAsync(key, page, perPage);
        }

        public Task RegisterSystemEventAsync(string eventType, object data = null)
        {
            return submitter.SendAsync(Event.Create("system", eventType, data));
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
