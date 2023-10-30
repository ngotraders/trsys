using System;

namespace Trsys.Models
{
    [Flags]
    public enum SecretKeyType
    {
        Publisher = 1,
        Subscriber = 1 << 1,
    }
}
