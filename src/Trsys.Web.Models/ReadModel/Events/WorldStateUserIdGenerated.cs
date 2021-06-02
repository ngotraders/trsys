using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class WorldStateUserIdGenerated : INotification, IEvent
    {
        public WorldStateUserIdGenerated(Guid id, string username, Guid userId)
        {
            Id = id;
            Username = username;
            UserId = userId;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Username { get; set; }
        public Guid UserId { get; set; }
    }
}