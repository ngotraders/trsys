using CQRSlite.Events;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class SecretKeyDescriptionChanged : IEvent
    {
        public SecretKeyDescriptionChanged(Guid id, string description)
        {
            Id = id;
            Description = description;
        }

        public Guid Id { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}