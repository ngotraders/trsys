using MediatR;
using Microsoft.Extensions.Logging;
using SqlStreamStore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Events;
using Trsys.Web.Models.ReadModel.Infrastructure;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Models.ReadModel.Handlers
{
    public class LogHandlers :
        INotificationHandler<LogNotification>,
        IRequestHandler<GetLogs, IEnumerable<LogDto>>
    {
        private static readonly TaskQueue quque = new();
        private readonly LogInMemoryDatabase db;
        private readonly ILogger<LogHandlers> logger;

        public LogHandlers(LogInMemoryDatabase db, ILogger<LogHandlers> logger)
        {
            this.db = db;
            this.logger = logger;
        }
        public Task Handle(LogNotification notification, CancellationToken cancellationToken)
        {
            return quque.Enqueue(() =>
            {
                foreach (var line in notification.Lines)
                {
                    var log = LogDto.Create(notification.RequestId, notification.Timestamp, notification.Key, notification.Version, line);
                    db.Add(log);
                    logger.Log(ConvertToLogLevel(log.LogType), $"{notification.Key}:{notification.Version}:{line}");
                }
            });
        }

        private LogLevel ConvertToLogLevel(string logType)
        {
            switch (logType)
            {
                case "DEBUG":
                    return LogLevel.Debug;
                case "INFO":
                    return LogLevel.Information;
                case "WARN":
                    return LogLevel.Warning;
                case "ERROR":
                    return LogLevel.Error;
            }
            return LogLevel.None;
        }

        public Task<IEnumerable<LogDto>> Handle(GetLogs request, CancellationToken cancellationToken)
        {
            var events = (string.IsNullOrEmpty(request.Source)
                ? db.All
                : db.BySource.TryGetValue(request.Source, out var list)
                ? list
                : new List<LogDto>())
                .AsEnumerable()
                .Reverse();
            if (request.PerPage > 0)
            {
                return Task.FromResult(events.Skip((request.Page - 1) * request.PerPage).Take(request.PerPage));
            }
            return Task.FromResult(events);
        }
    }
}
