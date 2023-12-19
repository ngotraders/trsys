using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using Trsys.Models.Events;

namespace Trsys.Models.WriteModel.Domain
{
    public class WorldStateAggregate : AggregateRoot
    {
        public static readonly Guid WORLD_STATE_ID = Guid.Parse("3502ca88-a8e7-4ea4-92dd-ce181e932c58");

        private readonly Dictionary<string, Guid?> _secretKeys = [];
        private readonly Dictionary<string, Guid?> _userNames = [];
        public void Apply(WorldStateSecretKeyIdGenerated e) => _secretKeys.Add(e.Key, e.SecretKeyId);
        public void Apply(WorldStateSecretKeyDeleted e) => _secretKeys.Remove(e.Key);
        public void Apply(WorldStateUserIdGenerated e)
        {
            _userNames.Add(e.Username, e.UserId);
        }
        public void Apply(WorldStateUserChanged e)
        {
            _userNames.Remove(e.OldUsername);
            _userNames.Add(e.NewUsername, e.UserId);
        }
        public void Apply(WorldStateUserDeleted e) => _userNames.Remove(e.Username);

        public WorldStateAggregate()
        {
        }

        public WorldStateAggregate(Guid id)
        {
            Id = id;
            ApplyChange(new WorldStateCreated(id));
        }

        public bool GenerateSecretKeyIdIfNotExists(string key, out Guid id)
        {
            if (_secretKeys.TryGetValue(key, out var secretKeyId))
            {
                id = secretKeyId.Value;
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

        public bool GenerateUserIdIfNotExists(string username, out Guid id)
        {
            if (_userNames.TryGetValue(username, out var userId))
            {
                id = userId.Value;
                return false;
            }
            id = Guid.NewGuid();
            ApplyChange(new WorldStateUserIdGenerated(Id, username, id));
            return true;
        }

        public bool UpdateUser(string oldUsername, string newUsername, Guid idToUpdate)
        {
            if (oldUsername == newUsername)
            {
                return true;
            }
            if (!_userNames.TryGetValue(oldUsername, out var id) || id != idToUpdate)
            {
                throw new InvalidOperationException("Data is not consistent.");
            }

            if (_userNames.TryGetValue(newUsername, out var _))
            {
                return false;
            }
            ApplyChange(new WorldStateUserChanged(Id, oldUsername, newUsername, idToUpdate));
            return true;
        }

        public void DeleteUser(string username, Guid idToDelete)
        {
            if (_userNames.TryGetValue(username, out var id))
            {
                if (id == idToDelete)
                {
                    ApplyChange(new WorldStateUserDeleted(Id, username));
                }
            }
        }
    }
}
