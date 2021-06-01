using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class CreateUserCommand : IRequest<Guid>
    {
        public CreateUserCommand(string name, string username, string passwordHash, string role)
        {
            Name = name;
            Username = username;
            PasswordHash = passwordHash;
            Role = role;
        }

        public string Name { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
