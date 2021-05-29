using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Services
{
    public class ReleaseSecretTokenResult : OperationResult
    {
        public string Key { get; private set; }
        public SecretKeyType KeyType { get; private set; }

        public static ReleaseSecretTokenResult Ok(string key, SecretKeyType keyType)
        {
            return new ReleaseSecretTokenResult()
            {
                Success = true,
                Key = key,
                KeyType = keyType
            };
        }
    }
}
