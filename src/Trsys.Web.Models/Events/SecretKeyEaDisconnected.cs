using MediatR;
using System;

namespace Trsys.Web.Models.Events
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