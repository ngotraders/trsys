using Microsoft.Extensions.Options;

namespace Trsys.Web.Authentication
{
    public class PostConfigureSecretTokenAuthenticationOptions : IPostConfigureOptions<SecretTokenAuthenticationSchemeOptions>
    {
        private readonly ISecretTokenStore store;

        public PostConfigureSecretTokenAuthenticationOptions(ISecretTokenStore store)
        {
            this.store = store;
        }
        public void PostConfigure(string name, SecretTokenAuthenticationSchemeOptions options)
        {
            options.Store = store;
        }
    }
}
