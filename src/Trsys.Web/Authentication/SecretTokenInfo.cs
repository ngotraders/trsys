using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Trsys.Web.Models;

namespace Trsys.Web.Authentication
{
    public class SecretTokenInfo
    {
        private readonly Tuple<string, SecretKeyType>[] secretKeyTypes = Enum.GetValues(typeof(SecretKeyType))
            .OfType<SecretKeyType>()
            .Select(key => Tuple.Create(Enum.GetName(typeof(SecretKeyType), key), key))
            .ToArray();

        public SecretTokenInfo(string secretKey, SecretKeyType keyType, string token)
        {
            SecretKey = secretKey;
            KeyType = keyType;
            Token = token;

            var principal = new ClaimsPrincipal();
            var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, secretKey) };
            foreach (var elem in secretKeyTypes)
            {
                if (keyType.HasFlag(elem.Item2))
                {
                    claims.Add(new Claim(ClaimTypes.Role, elem.Item1));
                }
            }
            principal.AddIdentity(new ClaimsIdentity(claims, "SecretToken"));
            Ticket = new AuthenticationTicket(principal, "SecretToken");

        }
        public string SecretKey { get; }
        public SecretKeyType KeyType { get; }
        public string Token { get; }
        public AuthenticationTicket Ticket { get; }

        public DateTime LastAccessed { get; private set; }

        public bool IsInUse()
        {
            return DateTime.UtcNow - LastAccessed.ToUniversalTime() < TimeSpan.FromSeconds(5);
        }

        public void Access()
        {
            LastAccessed = DateTime.UtcNow;
        }
    }
}
