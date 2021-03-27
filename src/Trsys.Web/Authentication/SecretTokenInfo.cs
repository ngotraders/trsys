using System;
using Trsys.Web.Models;

namespace Trsys.Web.Auth
{
    public class SecretTokenInfo
    {
        public string SecretKey { get; set; }
        public SecretKeyType KeyType { get; set; }
        public string Token { get; set; }
        public DateTime LastAccessed { get; set; }

        public bool IsInUse()
        {
            return DateTime.UtcNow - LastAccessed < TimeSpan.FromSeconds(5);
        }

        public void Access()
        {
            LastAccessed = DateTime.UtcNow;
        }
    }
}
