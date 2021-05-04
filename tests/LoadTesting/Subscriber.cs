using NBomber.Contracts;
using Serilog;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoadTesting
{
    public class Subscriber : TokenClientBase
    {
        private string orderText;
        private string orderHash;

        public Subscriber(string endpoint, string secretKey) : base(endpoint, secretKey)
        {
        }

        protected override async Task<Response> OnExecuteAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/orders");
            if (!string.IsNullOrEmpty(orderHash))
            {
                request.Headers.Add("If-None-Match", orderHash);
            }
            var res = await Client.SendAsync(request);
            if (res.StatusCode == HttpStatusCode.NotModified)
            {
                return Response.Ok();
            }
            if (res.IsSuccessStatusCode)
            {
                Response.Fail($"Order response is not valid. Status code = {res.StatusCode}");
            }

            orderHash = res.Headers.ETag.Tag;
            orderText = await res.Content.ReadAsStringAsync();
            Log.Logger.Information("Order changed: {0}", orderText);

            return Response.Ok();
        }
    }
}
