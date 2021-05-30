using MediatR;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class CreateSecretKeyCommand : IRequest
    {
        public CreateSecretKeyCommand(SecretKeyType? keyType, string key, string description, bool approve = false)
        {
            KeyType = keyType;
            Key = key;
            Description = description;
            Approve = approve;
        }

        public SecretKeyType? KeyType { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public bool Approve { get; set; }
    }
}
