using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.Events
{
    public class SecretKeyTokenGenerated : INotification, IEvent
    {
        public SecretKeyTokenGenerated(Guid id, string token)
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