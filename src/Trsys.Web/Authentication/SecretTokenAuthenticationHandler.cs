using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

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
            var ticket = await store.FindAsync(token);
            if (ticket == null)
            {
                return AuthenticateResult.Fail("unknown token.");
            }
            await Options.SecretKeyUsage.TouchAsync(ticket.Principal.Identity.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
