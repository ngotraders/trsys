using System;
using CQRSlite.Events;
using MediatR;

namespace Trsys.Models.Events
{
    public class UserDeleted : INotification, IEvent
    {
        public UserDeleted(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}