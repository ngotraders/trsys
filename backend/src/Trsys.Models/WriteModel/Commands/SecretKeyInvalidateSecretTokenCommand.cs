using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class SecretKeyInvalidateSecretTokenCommand : IRequest, IRetryableRequest
    {
        public SecretKeyInvalidateSecretTokenCommand(Guid id, string token)
        {
            Id = id;
            Token = token;
        }

        public Guid Id { get; set; }
        public string Token { get; set; }
    }
}
