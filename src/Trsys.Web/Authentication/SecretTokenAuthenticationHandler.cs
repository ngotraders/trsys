using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Authentication
{
    public class SecretTokenAuthenticationHandler : AuthenticationHandler<SecretTokenAuthenticationSchemeOptions>
    {
        public SecretTokenAuthenticationHandler(IOptionsMonitor<SecretTokenAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = Context.Request.Headers["X-Secret-Token"];
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail($"X-Secret-Token header is missing.");
            }

            var store = Options.Store;
            var tokenInfo = await store.FindInfoAsync(token);
            if (tokenInfo == null)
            {
                return AuthenticateResult.Fail("unknown token.");
            }

            var principal = new ClaimsPrincipal();
            var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, tokenInfo.SecretKey) };
            foreach (SecretKeyType key in Enum.GetValues(typeof(SecretKeyType)))
            {
                if (tokenInfo.KeyType.HasFlag(key))
                {
                    claims.Add(new Claim(ClaimTypes.Role, Enum.GetName(typeof(SecretKeyType), key)));
                }
            }
            principal.AddIdentity(new ClaimsIdentity(claims, Scheme.Name));
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
