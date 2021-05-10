using Newtonsoft.Json;
using System;
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
            return submitter.SendAsync(new Event()
            {
                EventType = $"system/{eventType}",
                Data = SerializeData(data),
            });
        }

        public Task RegisterEaEventAsync(string secretKey, string eventType, object data = null)
        {
            return submitter.SendAsync(new Event()
            {
                EventType = $"ea/{secretKey}/{eventType}",
                Data = SerializeData(data),
            });
        }

        public Task RegisterUserEventAsync(string username, string eventType, object data = null)
        {
            return submitter.SendAsync(new Event()
            {
                EventType = $"user/{username}/{eventType}",
                Data = SerializeData(data),
            });
        }

        private string SerializeData(object data)
        {
            if (data == null)
            {
                return null;
            }
            else if (data.GetType().IsPrimitive || data is string || data is decimal)
            {
                return data.ToString();
            }
            return JsonConvert.SerializeObject(data);
        }
    }
}
