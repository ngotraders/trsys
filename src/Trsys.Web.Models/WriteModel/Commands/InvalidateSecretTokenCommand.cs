using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class InvalidateSecretTokenCommand : IRequest
    {
        public InvalidateSecretTokenCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
