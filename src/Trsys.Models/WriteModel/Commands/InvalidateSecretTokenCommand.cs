using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class InvalidateSecretTokenCommand : IRequest, IRetryableRequest
    {
        public InvalidateSecretTokenCommand(Guid id, string token)
        {
            Id = id;
            Token = token;
        }

        public Guid Id { get; set; }
        public string Token { get; set; }
    }
}
