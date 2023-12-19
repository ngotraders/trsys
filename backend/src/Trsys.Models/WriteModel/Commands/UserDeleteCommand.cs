using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class UserDeleteCommand : IRequest, IRetryableRequest
    {
        public UserDeleteCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
