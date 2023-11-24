using CQRSlite.Domain;
using System;
using Trsys.Models.Events;

namespace Trsys.Models.WriteModel.Domain
{
    public class UserAggregate : AggregateRoot
    {
        private string _passwordHash;
        public void Apply(UserPasswordHashChanged e) => _passwordHash = e.PasswordHash;

        public UserAggregate()
        {
        }

        public UserAggregate(Guid id, string name, string username, string role)
        {
            Id = id;
            ApplyChange(new UserCreated(id, name, username, role));
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
