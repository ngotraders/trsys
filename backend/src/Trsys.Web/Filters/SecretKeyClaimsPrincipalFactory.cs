using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Trsys.Models;

namespace Trsys.Web.Filters;

public static class SecretKeyClaimsPrincipalFactory
{
    private static readonly Tuple<string, SecretKeyType>[] secretKeyTypes = Enum.GetValues(typeof(SecretKeyType))
        .OfType<SecretKeyType>()
        .Select(key => Tuple.Create(Enum.GetName(typeof(SecretKeyType), key)!, key))
        .ToArray();

    public static ClaimsPrincipal Create(Guid id, string key, SecretKeyType keyType)
    {
        var principal = new ClaimsPrincipal();
        var claims = new List<Claim>() {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimsIdentity.DefaultNameClaimType, key)
        };
        foreach (var elem in secretKeyTypes)
        {
            if (keyType.HasFlag(elem.Item2))
            {
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, elem.Item1));
            }
        }
        principal.AddIdentity(new ClaimsIdentity(claims, "SecretToken", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType));
        return principal;
    }
}
