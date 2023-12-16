using System;

namespace Trsys.Models
{
    [Flags]
    public enum SecretKeyType
    {
        Unknown = 0,
        Publisher = 1,
        Subscriber = 1 << 1,
    }
}
