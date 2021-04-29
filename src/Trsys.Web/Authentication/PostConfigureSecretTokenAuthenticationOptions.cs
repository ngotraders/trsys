using Microsoft.Extensions.Options;
using Trsys.Web.Models.SecretKeys;
using Trsys.Web.Services;

namespace Trsys.Web.Authentication
{
    public class PostConfigureSecretTokenAuthenticationOptions : IPostConfigureOptions<SecretTokenAuthenticationSchemeOptions>
    {
        private readonly IAuthenticationTicketStore store;
        private readonly ISecretKeyUsageStore usageStore;

        public PostConfigureSecretTokenAuthenticationOptions(IAuthenticationTicketStore store, ISecretKeyUsageStore usageStore)
        {
            this.store = store;
            this.usageStore = usageStore;
        }
        public void PostConfigure(string name, SecretTokenAuthenticationSchemeOptions options)
        {
            options.Store = store;
            options.SecretKeyUsage = usageStore;
        }
    }
}
