using System;
using CQRSlite.Events;
using MediatR;

namespace Trsys.Web.Models.Events
{
    public class UserCreated : INotification, IEvent
    {
        public UserCreated(Guid id, string name, string username, string role)
        {
            Id = id;
            Name = name;
            Username = username;
            Role = role;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}