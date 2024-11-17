using MediatR;
using System;

namespace Trsys.Models.Events
{
    public class SecretKeyEaConnected : INotification
    {
        public SecretKeyEaConnected(Guid id, string eaState)
        {
            Id = id;
            EaState = eaState;
        }

        public Guid Id { get; set; }
        public string EaState { get; }
    }
}