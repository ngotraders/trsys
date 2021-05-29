using System;

namespace Trsys.Web.Models.SecretKeys
{
    public class SecretKeyToken
    {
        public SecretKeyType? KeyType { get; set; }
        public string Key { get; set; }
        public bool IsValid { get; set; }
        public string Token { get; set; }
        public DateTime? LastAccessed { get; set; }

        public void Reset()
        {
            LastAccessed = null;
        }

        public void Touch()
        {
            LastAccessed = DateTime.UtcNow;
        }

        public bool IsInUse()
        {
            if (!LastAccessed.HasValue)
            {
                return false;
            }
            return DateTime.UtcNow - LastAccessed.Value.ToUniversalTime() < TimeSpan.FromSeconds(5);
        }

        public bool HasToken => !string.IsNullOrEmpty(Token);

        public void Approve()
        {
            IsValid = true;
        }

        public void Revoke()
        {
            IsValid = false;
        }

        public void ReleaseToken()
        {
            Token = null;
        }

        public string GenerateToken()
        {
            Token = Guid.NewGuid().ToString();
            return Token;
        }

        public static SecretKeyToken Create(string key, SecretKeyType? keyType)
        {
            return new SecretKeyToken()
            {
                Key = key,
                KeyType = keyType,
                IsValid = false,
            };
        }
    }
}
