using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Models.Events
{
    public class UserUserInfoUpdated : INotification, IEvent
    {
        public UserUserInfoUpdated(Guid id, string name, string emailAddress)
        {
            Id = id;
            Name = name;
            EmailAddress = emailAddress;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
    }
}