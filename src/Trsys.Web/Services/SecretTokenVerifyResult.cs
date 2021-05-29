using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Services
{
    public class SecretTokenVerifyResult : OperationResult
    {
        public string SecretKey { get; private set; }
        public SecretKeyType KeyType { get; private set; }

        public static SecretTokenVerifyResult Ok(string secretKey, SecretKeyType keyType)
        {
            return new SecretTokenVerifyResult()
            {
                Success = true,
                SecretKey= secretKey,
                KeyType = keyType,
            };
        }
    }
}