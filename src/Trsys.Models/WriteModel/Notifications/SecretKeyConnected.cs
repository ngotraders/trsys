using MediatR;
using System;

namespace Trsys.Models.WriteModel.Notifications
{
    public class SecretKeyConnected : INotification
    {
        public SecretKeyConnected(Guid id, string eaState, bool forcePublishEvent = false)
        {
            Id = id;
            EaState = eaState;
            ForcePublishEvent = forcePublishEvent;
        }

        public Guid Id { get; }
        public string EaState { get; }
        public bool ForcePublishEvent { get; }
    }
}
