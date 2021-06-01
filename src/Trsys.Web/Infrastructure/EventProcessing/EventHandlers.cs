using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Events;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure.EventProcessing
{
    public class EventHandlers :
        INotificationHandler<SystemEventNotification>,
        INotificationHandler<UserEventNotification>,
        INotificationHandler<EaEventNotification>,
        IRequestHandler<GetEvents, List<Event>>
    {
        private readonly EventService service;

        public EventHandlers(EventService service)
        {
            this.service = service;
        }
        public Task<List<Event>> Handle(GetEvents request, CancellationToken cancellationToken)
        {
            return service.SearchAsync(request.Source, request.Page, request.PerPage);
        }

        public Task Handle(SystemEventNotification notification, CancellationToken cancellationToken)
        {
            return service.RegisterSystemEventAsync(notification.Category, notification.EventType, notification.Data);
        }
        public Task Handle(UserEventNotification notification, CancellationToken cancellationToken)
        {
            return service.RegisterUserEventAsync(notification.Username, notification.EventType, notification.Data);
        }
        public Task Handle(EaEventNotification notification, CancellationToken cancellationToken)
        {
            return service.RegisterEaEventAsync(notification.Key, notification.EventType, notification.Data);
        }

    }
}
