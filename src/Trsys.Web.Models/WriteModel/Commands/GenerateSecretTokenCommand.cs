using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class GenerateSecretTokenCommand : IRequest<string>
    {
        public GenerateSecretTokenCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
