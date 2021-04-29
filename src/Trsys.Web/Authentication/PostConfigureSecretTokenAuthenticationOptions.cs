using Microsoft.Extensions.Options;
using Trsys.Web.Services;

namespace Trsys.Web.Authentication
{
    public class PostConfigureSecretTokenAuthenticationOptions : IPostConfigureOptions<SecretTokenAuthenticationSchemeOptions>
    {
        private readonly IAuthenticationTicketStore store;
        private readonly SecretKeyService service;

        public PostConfigureSecretTokenAuthenticationOptions(IAuthenticationTicketStore store, SecretKeyService service)
        {
            this.store = store;
            this.service = service;
        }
        public void PostConfigure(string name, SecretTokenAuthenticationSchemeOptions options)
        {
            options.Store = store;
            options.Service = service;
        }
    }
}
