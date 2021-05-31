using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class DisconnectSecretKeyCommand : IRequest
    {
        public DisconnectSecretKeyCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}