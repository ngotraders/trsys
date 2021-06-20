using System;

namespace Trsys.Web.Models.ReadModel.Dtos
{
    public class EventDto
    {
        public string Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string AggregateId { get; set; }
        public int Version { get; set; }
        public string EventType { get; set; }
        public string Data { get; set; }
    }
}
