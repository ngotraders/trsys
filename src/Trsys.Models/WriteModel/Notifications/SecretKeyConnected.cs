using MediatR;
using System;

namespace Trsys.Models.WriteModel.Notifications
{
    public class SecretKeyConnected : INotification
    {
        public SecretKeyConnected(Guid id, bool forcePublishEvent = false)
        {
            Id = id;
            ForcePublishEvent = forcePublishEvent;
        }

        public Guid Id { get; }
        public bool ForcePublishEvent { get; }
    }
}
