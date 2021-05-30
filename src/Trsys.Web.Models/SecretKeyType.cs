using System;

namespace Trsys.Web.Models
{
    [Flags]
    public enum SecretKeyType
    {
        Publisher = 1,
        Subscriber = 1 << 1,
    }
}
