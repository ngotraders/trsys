using CQRSlite.Domain;
using System;
using Trsys.Models.Events;

namespace Trsys.Models.WriteModel.Domain
{
    public class UserAggregate : AggregateRoot
    {
        private string _name;
        private string _emailAddress;
        private string _passwordHash;
        public void Apply(UserCreated e) => _name = e.Name;
        public void Apply(UserPasswordHashChanged e) => _passwordHash = e.PasswordHash;
        public void Apply(UserUserInfoUpdated e)
        {
            _name = e.Name;
            _emailAddress = e.EmailAddress;
        }

        public UserAggregate()
        {
        }

        public UserAggregate(Guid id, string name, string username, string role)
        {
            Id = id;
            ApplyChange(new UserCreated(id, name, username, role));
        }

        public void UpdateUserInfo(string name, string emailAddress)
        {
            if (name == _name && emailAddress == _emailAddress)
            {
                return;
            }
            ApplyChange(new UserUserInfoUpdated(Id, name, emailAddress));
        }


        public void ChangePasswordHash(string passwordHash)
        {
            if (_passwordHash != passwordHash)
            {
                ApplyChange(new UserPasswordHashChanged(Id, passwordHash));
            }
        }
    }
}
