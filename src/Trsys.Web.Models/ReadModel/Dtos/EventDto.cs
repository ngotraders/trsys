using Newtonsoft.Json;
using System;

namespace Trsys.Web.Models.ReadModel.Dtos
{
    public class EventDto
    {
        public EventDto()
        {
            Id = Guid.Empty.ToString();
            Timestamp = DateTimeOffset.MinValue;
        }

        public EventDto(string source, string eventType, string data)
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

        public static EventDto Create(string source, string eventType, object data)
        {
            return new EventDto(source, eventType, SerializeData(data));
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
