using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class ConnectSecretKeyCommand : IRequest
    {
        public ConnectSecretKeyCommand(Guid id, string token)
        {
            Id = id;
            Token = token;
        }

        public Guid Id { get; set; }
        public string Token { get; set; }
    }
}
