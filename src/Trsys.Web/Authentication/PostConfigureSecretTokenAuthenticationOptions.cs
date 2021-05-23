using Microsoft.Extensions.Options;

namespace Trsys.Web.Authentication
{
    public class PostConfigureSecretTokenAuthenticationOptions : IPostConfigureOptions<SecretTokenAuthenticationSchemeOptions>
    {
        private readonly IAuthenticationTicketStore store;

        public PostConfigureSecretTokenAuthenticationOptions(IAuthenticationTicketStore store)
        {
            this.store = store;
        }
        public void PostConfigure(string name, SecretTokenAuthenticationSchemeOptions options)
        {
            options.Store = store;
        }
    }
}
