using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class DeleteSecretKeyCommand : IRequest, IRetryableRequest
    {
        public DeleteSecretKeyCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
