using System;
using CQRSlite.Events;
using MediatR;

namespace Trsys.Models.Events
{
    public class UserPasswordHashChanged : INotification, IEvent
    {
        public UserPasswordHashChanged(Guid id, string password)
        {
            Id = id;
            PasswordHash = password;
        }

        public Guid Id { get; set; }
        public string PasswordHash { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}