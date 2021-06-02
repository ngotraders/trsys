using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class WorldStateUserDeleted : INotification, IEvent
    {
        public WorldStateUserDeleted(Guid id, string username)
        {
            Id = id;
            Username = username;
        }

        public Guid Id { get; set; }
        public string Username { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
