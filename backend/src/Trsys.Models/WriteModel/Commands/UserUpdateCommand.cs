using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class UserUpdateCommand : IRequest, IRetryableRequest
    {
        public UserUpdateCommand(Guid id, string name, string username, string emailAddress, string passwordHash, string role)
        {
            Id = id;
            Name = name;
            Username = username;
            EmailAddress = emailAddress;
            PasswordHash = passwordHash;
            Role = role;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
