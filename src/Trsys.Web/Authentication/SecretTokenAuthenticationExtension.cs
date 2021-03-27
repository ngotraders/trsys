using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Trsys.Web.Auth
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
            return builder.AddScheme<SecretTokenAuthenticationSchemeOptions, SecretTokenAuthenticationHandler>(authenticationScheme, displayName,
                (SecretTokenAuthenticationSchemeOptions options) =>
                {
                    configureOptions?.Invoke(options);

                    if (options.StoreFactory == null)
                    {
                        options.StoreFactory = () =>
                        {
                            var serviceProvider = builder.Services.BuildServiceProvider();
                            return serviceProvider.GetService<ISecretTokenStore>();
                        };
                    }
                });
        }
    }
}
