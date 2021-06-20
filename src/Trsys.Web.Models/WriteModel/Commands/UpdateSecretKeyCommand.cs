using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class UpdateSecretKeyCommand: IRequest, IRetryableRequest
    {
        public UpdateSecretKeyCommand(Guid id, SecretKeyType? keyType, string description, bool? approve = null)
        {
            Id = id;
            KeyType = keyType;
            Description = description;
            Approve = approve;
        }

        public Guid Id { get; set; }
        public SecretKeyType? KeyType { get; set; }
        public string Description { get; set; }
        public bool? Approve { get; set; }
    }
}
