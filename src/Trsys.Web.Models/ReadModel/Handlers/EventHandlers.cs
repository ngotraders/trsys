using MediatR;
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
    public class EventHandlers :
        INotificationHandler<SystemEventNotification>,
        INotificationHandler<UserEventNotification>,
        INotificationHandler<EaEventNotification>,
        IRequestHandler<GetEvents, IEnumerable<EventDto>>
    {
        private static readonly TaskQueue quque = new();
        private readonly EventInMemoryDatabase db;

        public EventHandlers(EventInMemoryDatabase db)
        {
            this.db = db;
        }

        public Task Handle(SystemEventNotification notification, CancellationToken cancellationToken)
        {
            return quque.Enqueue(() =>
            {
                db.Add(EventDto.Create($"system/{notification.Category}", notification.EventType, notification.Data));
            });
        }
        public Task Handle(UserEventNotification notification, CancellationToken cancellationToken)
        {
            return quque.Enqueue(() =>
            {
                db.Add(EventDto.Create($"user/{notification.Username}", notification.EventType, notification.Data));
            });
        }
        public Task Handle(EaEventNotification notification, CancellationToken cancellationToken)
        {
            return quque.Enqueue(() =>
            {
                db.Add(EventDto.Create($"ea/{notification.Key}", notification.EventType, notification.Data));
            });
        }

        public Task<IEnumerable<EventDto>> Handle(GetEvents request, CancellationToken cancellationToken)
        {
            var events = (string.IsNullOrEmpty(request.Source)
                ? db.All
                : db.BySource.TryGetValue(request.Source, out var list)
                ? list
                : new List<EventDto>())
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
