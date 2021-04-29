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

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = Context.Request.Headers["X-Secret-Token"];
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail($"X-Secret-Token header is missing."));
            }

            var store = Options.Store;
            var ticket = store.Find(token);
            if (ticket == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("unknown token."));
            }
            Options.SecretKeyUsage.Touch(ticket.Principal.Identity.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
