using System;

namespace Trsys.Web.Models
{
    public enum SecretKeyType
    {
        Publisher,
        Subscriber
    }

    public class SecretKey
    {
        public int Id { get; set; }
        public SecretKeyType KeyType { get; set; }
        public string Key { get; set; }

        public bool IsValid { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public string ValidToken { get; set; }

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

        public void UpdateToken(string token)
        {
            ValidToken = token;
        }
    }
}
