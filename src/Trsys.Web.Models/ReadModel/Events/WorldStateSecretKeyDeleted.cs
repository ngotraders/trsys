using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class WorldStateSecretKeyDeleted : INotification, IEvent
    {
        public WorldStateSecretKeyDeleted(Guid id, string key)
        {
            Id = id;
            Key = key;
        }

        public Guid Id { get; set; }
        public string Key { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
