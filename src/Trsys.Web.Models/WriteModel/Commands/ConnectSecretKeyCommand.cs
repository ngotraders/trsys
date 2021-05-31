using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class ConnectSecretKeyCommand : IRequest
    {
        public ConnectSecretKeyCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
