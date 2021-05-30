using CQRSlite.Events;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class SecretKeyEaDisconnected : IEvent
    {
        public SecretKeyEaDisconnected(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}