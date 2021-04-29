using System;

namespace Trsys.Web.Models.SecretKeys
{
    public class SecretKeyUsage
    {
        public string SecretKey { get; set; }
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
    }
}
