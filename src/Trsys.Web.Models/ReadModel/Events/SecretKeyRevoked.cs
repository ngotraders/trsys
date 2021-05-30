using CQRSlite.Events;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class SecretKeyRevoked : IEvent
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