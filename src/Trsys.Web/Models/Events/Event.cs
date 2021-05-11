using Newtonsoft.Json;
using System;

namespace Trsys.Web.Models.Events
{
    public class Event
    {
        public Event()
        {
            Id = Guid.Empty.ToString();
            Timestamp = DateTimeOffset.MinValue;
        }

        public Event(string source, string eventType, string data)
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.UtcNow;
            Source = source;
            EventType = eventType;
            Data = data;
        }

        public string Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Source { get; set; }
        public string EventType { get; set; }
        public string Data { get; set; }

        public static Event Create<T>(string source, string eventType, T data)
        {
            return new Event(source, eventType, SerializeData(data));
        }

        private static string SerializeData(object data)
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
