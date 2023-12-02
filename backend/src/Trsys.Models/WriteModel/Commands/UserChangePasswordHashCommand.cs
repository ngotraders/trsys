using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class UserChangePasswordHashCommand : IRequest, IRetryableRequest
    {
        public UserChangePasswordHashCommand(Guid id, string passwordHash)
        {
            Id = id;
            PasswordHash = passwordHash;
        }

        public Guid Id { get; set; }
        public string PasswordHash { get; set; }
    }
}
