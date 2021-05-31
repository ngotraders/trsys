using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using Trsys.Web.Models.ReadModel.Events;

namespace Trsys.Web.Models.WriteModel.Domain
{
    public class WorldStateAggregate : AggregateRoot
    {
        public static Guid WORLD_STATE_ID = Guid.Parse("3502ca88-a8e7-4ea4-92dd-ce181e932c58");

        public WorldStateAggregate()
        {
        }

        public WorldStateAggregate(Guid id)
        {
            Id = id;
            ApplyChange(new WorldStateCreated(id));
        }

        private Dictionary<string, Guid> _secretKeys = new Dictionary<string, Guid>();
        public void Apply(WorldStateSecretKeyIdGenerated e) => _secretKeys.Add(e.Key, e.SecretKeyId);
        public void Apply(WorldStateSecretKeyDeleted e) => _secretKeys.Remove(e.Key);

        public bool GenerateSecretKeyIdIfNotExists(string key, out Guid id)
        {
            if (_secretKeys.TryGetValue(key, out id))
            {
                return false;
            }
            id = Guid.NewGuid();
            ApplyChange(new WorldStateSecretKeyIdGenerated(Id, key, id));
            return true;
        }

        public void DeleteSecretKey(string key, Guid idToDelete)
        {
            if (_secretKeys.TryGetValue(key, out var id))
            {
                if (id == idToDelete)
                {
                    ApplyChange(new WorldStateSecretKeyDeleted(Id, key));
                }
            }
        }
    }
}
