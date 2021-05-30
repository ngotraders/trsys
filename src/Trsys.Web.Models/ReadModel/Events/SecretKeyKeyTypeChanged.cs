using CQRSlite.Events;
using System;
using Trsys.Web.Models.WriteModel.Domain;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class SecretKeyKeyTypeChanged : IEvent
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