using System;

namespace Trsys.Web.Models.Events
{
    public class Event
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public string EventType { get; set; }
        public string Data { get; set; }
    }
}
