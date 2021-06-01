using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Services
{
    public class EventService
    {
        private readonly IEventRepository repository;

        public EventService(IEventRepository repository)
        {
            this.repository = repository;
        }

        public Task<List<Event>> SearchAsync(string source, int page, int perPage)
        {
            return repository.SearchAsync(source, page, perPage);
        }

        public Task RegisterSystemEventAsync(string category, string eventType, object data = null)
        {
            return repository.SaveAsync(Event.Create($"system/{category}", eventType, data));
        }

        public Task RegisterEaEventAsync(string secretKey, string eventType, object data = null)
        {
            return repository.SaveAsync(Event.Create($"ea/{secretKey}", eventType, data));
        }

        public Task RegisterUserEventAsync(string username, string eventType, object data = null)
        {
            return repository.SaveAsync(Event.Create($"user/{username}", eventType, data));
        }
    }
}
