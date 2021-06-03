using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Events;
using Trsys.Web.Models.ReadModel.Infrastructure;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Models.ReadModel.Handlers
{
    public class EventHandlers :
        INotificationHandler<SystemEventNotification>,
        INotificationHandler<UserEventNotification>,
        INotificationHandler<EaEventNotification>,
        IRequestHandler<GetEvents, List<EventDto>>
    {
        private readonly EventInMemoryDatabase db;

        public EventHandlers(EventInMemoryDatabase db)
        {
            this.db = db;
        }
        public Task<List<EventDto>> Handle(GetEvents request, CancellationToken cancellationToken)
        {
            var events = string.IsNullOrEmpty(request.Source) ? db.All : db.BySource[request.Source];
            if (request.PerPage > 0)
            {
                return Task.FromResult(events.Skip((request.Page - 1) * request.PerPage).Take(request.PerPage).ToList());
            }
            return Task.FromResult(events);
        }

        public Task Handle(SystemEventNotification notification, CancellationToken cancellationToken)
        {
            db.Add(EventDto.Create($"system/{notification.Category}", notification.EventType, notification.Data));
            return Task.CompletedTask;
        }
        public Task Handle(UserEventNotification notification, CancellationToken cancellationToken)
        {
            db.Add(EventDto.Create($"user/{notification.Username}", notification.EventType, notification.Data));
            return Task.CompletedTask;
        }
        public Task Handle(EaEventNotification notification, CancellationToken cancellationToken)
        {
            db.Add(EventDto.Create($"ea/{notification.Key}", notification.EventType, notification.Data));
            return Task.CompletedTask;
        }

    }
}
