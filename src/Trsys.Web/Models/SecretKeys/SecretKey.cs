using System;

namespace Trsys.Web.Models.SecretKeys
{
    [Flags]
    public enum SecretKeyType
    {
        Publisher = 1,
        Subscriber = 1 << 1,
    }

    public class SecretKey
    {
        public int Id { get; set; }
        public SecretKeyType? KeyType { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }

        public bool IsValid { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public string ValidToken { get; set; }

        public bool HasToken => !string.IsNullOrEmpty(ValidToken);

        public void Approve()
        {
            IsValid = true;
            ApprovedAt = DateTime.UtcNow;
        }

        public void Revoke()
        {
            IsValid = false;
            ApprovedAt = null;
            ValidToken = null;
        }

        public void ReleaseToken()
        {
            ValidToken = null;
        }

        public string GenerateToken()
        {
            ValidToken = Guid.NewGuid().ToString();
            return ValidToken;
        }

        public static SecretKey Create(SecretKeyType keyType)
        {
            return Create(Guid.NewGuid().ToString(), keyType, null);
        }

        public static SecretKey Create(string key, SecretKeyType? keyType, string description)
        {
            return new SecretKey()
            {
                Key = key,
                KeyType = keyType,
                Description = description,
                IsValid = false,
            };
        }
    }
}
