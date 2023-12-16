using System;
using Microsoft.AspNetCore.Identity;

namespace Trsys.Web.Identity;

public class TrsysRole : IdentityRole<Guid>
{
    public TrsysRole()
    {
    }

    public TrsysRole(string roleName) : base(roleName)
    {
    }
}