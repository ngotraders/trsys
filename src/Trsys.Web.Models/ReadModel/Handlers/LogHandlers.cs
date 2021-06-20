﻿using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Infrastructure;
using Trsys.Web.Models.ReadModel.Notifications;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Models.ReadModel.Handlers
{
    public class LogHandlers :
        INotificationHandler<LogNotification>,
        IRequestHandler<GetLogs, IEnumerable<LogDto>>
    {
        private readonly ILogDatabase db;

        public LogHandlers(ILogDatabase db)
        {
            this.db = db;
        }

        public Task Handle(LogNotification notification, CancellationToken cancellationToken)
        {
            return db.AddRangeAsync(notification.Lines.Select(line => LogDto.Create(notification.RequestId, notification.Timestamp, notification.Key, notification.Version, line)));
        }

        public Task<IEnumerable<LogDto>> Handle(GetLogs request, CancellationToken cancellationToken)
        {
            return db.SearchAsync(request.Source, request.Page, request.PerPage);
        }
    }
}
