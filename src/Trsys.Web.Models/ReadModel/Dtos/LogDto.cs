using System;
using System.Linq;

namespace Trsys.Web.Models.ReadModel.Dtos
{
    public class LogDto
    {
        private static string[] LogTypes = new[]
        {
            "DEBUG",
            "INFO",
            "WARN",
            "ERROR",
        };

        public string Key { get; set; }
        public string Version { get; set; }
        public DateTimeOffset Received { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string LogType { get; set; }
        public string Data { get; set; }

        public static LogDto Create(string key, string version, DateTimeOffset received, string line)
        {
            var splitted = line.Split(":");
            if (splitted.Length < 3 || !long.TryParse(splitted[0], out var tick) || !LogTypes.Contains(splitted[1]))
            {
                return new LogDto()
                {
                    Key = key,
                    Version = version,
                    Received = received,
                    Timestamp = received,
                    LogType = "UNKNOWN",
                    Data = line,
                };
            }
            return new LogDto()
            {
                Key = key,
                Version = version,
                Received = received,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(tick),
                LogType = splitted[1],
                Data = line,
            };
        }
    }
}
