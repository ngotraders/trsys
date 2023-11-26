using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class SecretKeyDeleteCommand : IRequest, IRetryableRequest
    {
        public SecretKeyDeleteCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
