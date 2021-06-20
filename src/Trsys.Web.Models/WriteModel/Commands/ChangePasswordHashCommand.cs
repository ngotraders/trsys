using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class ChangePasswordHashCommand : IRequest, IRetryableRequest
    {
        public ChangePasswordHashCommand(Guid id, string passwordHash)
        {
            Id = id;
            PasswordHash = passwordHash;
        }

        public Guid Id { get; set; }
        public string PasswordHash { get; set; }
    }
}
