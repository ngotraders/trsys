using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Models.Events
{
    public class WorldStateUserChanged : INotification, IEvent
    {
        public WorldStateUserChanged(Guid id, string oldUsername, string newUsername, Guid userId)
        {
            Id = id;
            OldUsername = oldUsername;
            NewUsername = newUsername;
            UserId = userId;
        }

        public Guid Id { get; set; }
        public string OldUsername { get; set; }
        public string NewUsername { get; set; }
        public Guid UserId { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
