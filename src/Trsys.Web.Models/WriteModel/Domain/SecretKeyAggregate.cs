using CQRSlite.Domain;
using System;
using Trsys.Web.Models.ReadModel.Events;

namespace Trsys.Web.Models.WriteModel.Domain
{
    public class SecretKeyAggregate : AggregateRoot
    {
        private string _key;
        private bool _approved;
        private SecretKeyType? _keyType;
        private string _description;
        private string _token;
        private bool _connected;
        private bool _deleted;

        public bool IsApproved => _approved;
        public string Key => _key;

        public void Apply(SecretKeyCreated e) => _key = e.Key;
        public void Apply(SecretKeyApproved e) => _approved = true;
        public void Apply(SecretKeyRevoked e) => _approved = false;
        public void Apply(SecretKeyKeyTypeChanged e) => _keyType = e.KeyType;
        public void Apply(SecretKeyDescriptionChanged e) => _description = e.Description;
        public void Apply(SecretKeyTokenGenerated e) => _token = e.Token;
        public void Apply(SecretKeyTokenInvalidated e) => _token = null;
        public void Apply(SecretKeyEaConnected e) => _connected = true;
        public void Apply(SecretKeyEaDisconnected e) => _connected = false;
        public void Apply(SecretKeyDeleted e) => _deleted = true;

        public SecretKeyAggregate()
        {
        }

        public SecretKeyAggregate(Guid id, string key)
        {
            Id = id;
            ApplyChange(new SecretKeyCreated(id, key));
        }

        public void ChangeKeyType(SecretKeyType keyType)
        {
            if (_keyType.HasValue && _keyType == keyType)
            {
                return;
            }
            if (_approved)
            {
                throw new InvalidOperationException("Cannot change key type if secret key is approved.");
            }
            ApplyChange(new SecretKeyKeyTypeChanged(Id, keyType));
        }

        public void ChangeDescription(string description)
        {
            if (_description == description)
            {
                return;
            }
            ApplyChange(new SecretKeyDescriptionChanged(Id, description));
        }

        public void Approve()
        {
            if (!_keyType.HasValue)
            {
                throw new InvalidOperationException("Cannot approve if key type is not set.");
            }
            if (!_approved)
            {
                ApplyChange(new SecretKeyApproved(Id));
            }
        }

        public void Revoke()
        {
            if (_approved)
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    InvalidateToken();
                }
                ApplyChange(new SecretKeyRevoked(Id));
            }
        }

        public string GenerateToken()
        {
            if (_connected)
            {
                throw new InvalidOperationException("Ea is already connected.");
            }
            if (!_approved)
            {
                throw new InvalidOperationException("Cannot generate token if secret key is approved.");
            }
            if (!string.IsNullOrEmpty(_token))
            {
                ApplyChange(new SecretKeyTokenInvalidated(Id, _token));
            }
            var token = Guid.NewGuid().ToString();
            ApplyChange(new SecretKeyTokenGenerated(Id, token));
            return token;
        }

        public void InvalidateToken()
        {
            if (string.IsNullOrEmpty(_token))
            {
                throw new InvalidOperationException("Token is not generated yet.");
            }
            if (_connected)
            {
                ApplyChange(new SecretKeyEaDisconnected(Id));
            }

            ApplyChange(new SecretKeyTokenInvalidated(Id, _token));
        }

        public void Connect(string token)
        {
            if (!_connected && _token == token)
            {
                ApplyChange(new SecretKeyEaConnected(Id));
            }
        }
        public void Disconnect(string token)
        {
            if (_connected && _token == token)
            {
                ApplyChange(new SecretKeyEaDisconnected(Id));
            }
        }
        public void Delete()
        {
            if (!_deleted)
            {
                if (_approved)
                {
                    throw new InvalidOperationException("Cannot delete secret key if approved.");
                }
                ApplyChange(new SecretKeyDeleted(Id));
            }
        }
    }
}
