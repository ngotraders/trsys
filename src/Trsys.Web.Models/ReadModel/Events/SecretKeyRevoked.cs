using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class SecretKeyRevoked : INotification, IEvent
    {
        public SecretKeyRevoked(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}