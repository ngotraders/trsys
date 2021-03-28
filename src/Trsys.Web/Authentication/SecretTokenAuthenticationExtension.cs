using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Trsys.Web.Authentication
{
    public static class SecretTokenAuthenticationExtension
    {
        public static AuthenticationBuilder AddSecretTokenAuthentication(this AuthenticationBuilder builder)
        {
            return builder.AddSecretTokenAuthentication("SecretToken", null, null);
        }

        public static AuthenticationBuilder AddSecretTokenAuthentication(this AuthenticationBuilder builder,
            string authenticationScheme,
            string displayName,
            Action<SecretTokenAuthenticationSchemeOptions> configureOptions)
        {
            builder.Services.AddSingleton<IPostConfigureOptions<SecretTokenAuthenticationSchemeOptions>, PostConfigureSecretTokenAuthenticationOptions>();
            return builder.AddScheme<SecretTokenAuthenticationSchemeOptions, SecretTokenAuthenticationHandler>(authenticationScheme, displayName,
                (options) =>
                {
                    configureOptions?.Invoke(options);
                });
        }
    }
}
