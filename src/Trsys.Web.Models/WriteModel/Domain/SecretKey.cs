using CQRSlite.Domain;
using System;
using Trsys.Web.Models.ReadModel.Events;

namespace Trsys.Web.Models.WriteModel.Domain
{
    public class SecretKey : AggregateRoot
    {
        private bool _approved;
        private SecretKeyType? _keyType;
        private string _token;
        private bool _connected;

        protected void Apply(SecretKeyApproved e)
        {
            _approved = true;
        }

        protected void Apply(SecretKeyRevoked e)
        {
            _approved = false;
        }

        protected void Apply(SecretKeyKeyTypeChanged e)
        {
            _keyType = e.KeyType;
        }

        protected void Apply(SecretKeyTokenGenerated e)
        {
            _token = e.Token;
        }

        protected void Apply(SecretKeyTokenInvalidated e)
        {
            _token = null;
        }

        protected void Apply(SecretKeyEaConnected e)
        {
            _connected = true;
        }

        protected void Apply(SecretKeyEaDisconnected e)
        {
            _connected = false;
        }

        public SecretKey(Guid id, string key)
        {
            Id = id;
            ApplyChange(new SecretKeyCreated(id, key));
        }

        public void ChangeKeyType(SecretKeyType keyType)
        {
            if (_approved)
            {
                throw new InvalidOperationException("Cannot change key type if secret key is approved.");
            }
            ApplyChange(new SecretKeyKeyTypeChanged(Id, keyType));
        }

        public void ChangeDescription(string description)
        {
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
                ApplyChange(new SecretKeyRevoked(Id));
            }
        }

        public void GenerateToken()
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
        }

        public void InvalidateToken()
        {
            if (string.IsNullOrEmpty(_token))
            {
                throw new InvalidOperationException("Token is not generated yet.");
            }
            ApplyChange(new SecretKeyTokenInvalidated(Id, _token));
        }

        public void Connect()
        {
            if (!_connected)
            {
                ApplyChange(new SecretKeyEaConnected(Id));
            }
        }
        public void Disconnect()
        {
            if (_connected)
            {
                ApplyChange(new SecretKeyEaDisconnected(Id));
            }
        }
    }
}
