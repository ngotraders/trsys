namespace Trsys.Web.Models.WriteModel.Commands
{
    public class CreateSecretKeyIfNotExistsCommand : CreateSecretKeyCommand
    {
        public CreateSecretKeyIfNotExistsCommand(SecretKeyType? keyType, string key, string description, bool? approve = null) : base(keyType, key, description, approve)
        {
        }
    }
}
