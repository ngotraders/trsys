using System;

namespace Trsys.Web.Models.Messaging
{
    public class PublishingMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }
}
