using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Models.Events
{
    public class SecretKeyTokenInvalidated : INotification, IEvent
    {
        public SecretKeyTokenInvalidated(Guid id, string token)
        {
            Id = id;
            Token = token;
        }

        public Guid Id { get; set; }
        public string Token { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}