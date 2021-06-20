using MediatR;
using System;

namespace Trsys.Web.Models.Events
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