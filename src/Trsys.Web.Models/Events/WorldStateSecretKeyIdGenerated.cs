using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.Events
{
    public class WorldStateSecretKeyIdGenerated : INotification, IEvent
    {
        public WorldStateSecretKeyIdGenerated(Guid id, string key, Guid secretKeyId)
        {
            Id = id;
            Key = key;
            SecretKeyId = secretKeyId;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Key { get; set; }
        public Guid SecretKeyId { get; set; }
    }
}