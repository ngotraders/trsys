using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class SecretKeyKeyTypeChanged : INotification, IEvent
    {
        public SecretKeyKeyTypeChanged(Guid id, SecretKeyType keyType)
        {
            Id = id;
            KeyType = keyType;
        }

        public Guid Id { get; set; }
        public SecretKeyType KeyType { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}