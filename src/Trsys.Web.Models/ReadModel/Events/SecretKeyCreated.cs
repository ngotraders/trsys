using CQRSlite.Events;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class SecretKeyCreated : IEvent
    {
        public SecretKeyCreated(Guid id, string key)
        {
            Id = id;
            Key = key;
        }

        public Guid Id { get; set; }
        public string Key { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}