using System;
using System.Net.Http;

namespace LoadTesting
{
    public static class HttpClientFactory
    {
        public static HttpClient Create(string endpoint, bool useCookies)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = useCookies,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true,
            };
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(endpoint)
            };
            return client;
        }
    }
}
