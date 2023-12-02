namespace Trsys.Models.WriteModel.Commands
{
    public class SecretKeyCreateIfNotExistsCommand : SecretKeyCreateCommand
    {
        public SecretKeyCreateIfNotExistsCommand(SecretKeyType? keyType, string key, string description, bool? approve = null) : base(keyType, key, description, approve)
        {
        }
    }
}
