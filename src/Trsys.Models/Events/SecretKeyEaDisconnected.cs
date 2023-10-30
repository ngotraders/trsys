using MediatR;
using System;

namespace Trsys.Models.Events
{
    public class SecretKeyEaDisconnected : INotification
    {
        public SecretKeyEaDisconnected(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}