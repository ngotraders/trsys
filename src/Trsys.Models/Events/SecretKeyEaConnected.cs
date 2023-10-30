using MediatR;
using System;

namespace Trsys.Models.Events
{
    public class SecretKeyEaConnected : INotification
    {
        public SecretKeyEaConnected(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}