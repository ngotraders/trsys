using Newtonsoft.Json;
using System.Threading.Tasks;
using Trsys.Web.Models.Events;

namespace Trsys.Web.Services
{
    public class EventService
    {
        private readonly IEventSubmitter submitter;

        public EventService(IEventSubmitter submitter)
        {
            this.submitter = submitter;
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
