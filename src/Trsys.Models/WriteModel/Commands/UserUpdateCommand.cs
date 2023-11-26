using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class UserUpdateCommand : IRequest, IRetryableRequest
    {
        public UserUpdateCommand(Guid id, string name, string emailAddress)
        {
            Id = id;
            Name = name;
            EmailAddress = emailAddress;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
    }
}
