using System;
using System.ComponentModel.DataAnnotations.Schema;
using Trsys.Web.Models;

namespace Trsys.Web.Authentication
{
    public class SecretTokenInfo
    {
        public string SecretKey { get; set; }
        public SecretKeyType KeyType { get; set; }
        public string Token { get; set; }
        public DateTime LastAccessed { get; set; }

        [NotMapped]
        public bool IsInUse { get; set; }

        public void SetIsInUse()
        {
            IsInUse = DateTime.UtcNow - LastAccessed.ToUniversalTime() < TimeSpan.FromSeconds(5);
        }

        public void Access()
        {
            LastAccessed = DateTime.UtcNow;
        }
    }
}
