using System;
using Microsoft.AspNetCore.Identity;

namespace Trsys.Web.Identity;

public class TrsysUser : IdentityUser<Guid>
{
    public TrsysUser()
    {
    }

    public TrsysUser(string userName) : base(userName)
    {
    }

    public string? Name { get; set; }
    public string? Role { get; set; }
}