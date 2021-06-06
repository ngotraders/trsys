using System;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public class PublishingMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }
}
