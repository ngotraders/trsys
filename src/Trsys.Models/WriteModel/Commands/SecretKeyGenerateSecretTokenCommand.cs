using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class SecretKeyGenerateSecretTokenCommand : IRequest<string>, IRetryableRequest
    {
        public SecretKeyGenerateSecretTokenCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
