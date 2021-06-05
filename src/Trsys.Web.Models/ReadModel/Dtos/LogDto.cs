using System;

namespace Trsys.Web.Models.ReadModel.Dtos
{
    public class LogDto
    {
        public string Key { get; set; }
        public DateTimeOffset Received { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Version { get; set; }
        public string LogType { get; set; }
        public string Data { get; set; }

        public static LogDto Create(string key, DateTimeOffset received, string line)
        {
            var splitted = line.Split(":");
            return new LogDto();
        }
    }
}
