using MediatR;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class LogNotification: INotification
    {
        public LogNotification(string requestId, string key, string version, string[] lines)
        {
            RequestId = requestId;
            Key = key;
            Version = version;
            Timestamp = DateTimeOffset.UtcNow;
            Lines = lines;
        }

        public string RequestId { get; set; }
        public string Key { get; set; }
        public string Version { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string[] Lines { get; set; }
    }
}
