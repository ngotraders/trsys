using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Services
{
    public class GenerateSecretTokenResult
    {
        public bool Success { get; set; }
        public bool InUse { get; set; }
        public string Token { get; set; }
        public string Key { get; set; }
        public SecretKeyType KeyType { get; set; }

        public static GenerateSecretTokenResult InvalidSecretKey()
        {
            return new GenerateSecretTokenResult()
            {
                Success = false
            };
        }

        public static GenerateSecretTokenResult SecretKeyInUse()
        {
            return new GenerateSecretTokenResult()
            {
                Success = false,
                InUse = true,
            };
        }

        public static GenerateSecretTokenResult Ok(string token, string key, SecretKeyType keyType)
        {
            return new GenerateSecretTokenResult()
            {
                Success = true,
                Token = token,
                Key = key,
                KeyType = keyType,
            };
        }
    }
}
