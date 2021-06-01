using System;
using CQRSlite.Events;
using MediatR;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class UserCreated : INotification, IEvent
    {
        public UserCreated(Guid id, string name, string username)
        {
            Id = id;
            Name = name;
            Username = username;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}