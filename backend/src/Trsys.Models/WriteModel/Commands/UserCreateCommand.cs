using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class UserCreateCommand : IRequest<Guid>, IRetryableRequest
    {
        public UserCreateCommand(string name, string username, string emailAddress, string passwordHash, string role)
        {
            Name = name;
            Username = username;
            EmailAddress = emailAddress;
            PasswordHash = passwordHash;
            Role = role;
        }

        public string Name { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
