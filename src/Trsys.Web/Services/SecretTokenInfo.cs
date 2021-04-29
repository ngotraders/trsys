using System;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Services
{
    public class SecretTokenInfo
    {
        public SecretTokenInfo(string secretKey, SecretKeyType keyType, string token)
        {
            SecretKey = secretKey;
            KeyType = keyType;
            Token = token;
        }

        public string SecretKey { get; }
        public SecretKeyType KeyType { get; }
        public string Token { get; }

        public DateTime LastAccessed { get; private set; }

        public bool IsInUse()
        {
            return DateTime.UtcNow - LastAccessed.ToUniversalTime() < TimeSpan.FromSeconds(5);
        }

        public void Access()
        {
            LastAccessed = DateTime.UtcNow;
        }
    }
}
