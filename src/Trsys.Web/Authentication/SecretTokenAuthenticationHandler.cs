using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
            principal.AddIdentity(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, tokenInfo.SecretKey),
                new Claim(ClaimTypes.Role, Enum.GetName(typeof(SecretKeyType), tokenInfo.KeyType)),
            }));
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
