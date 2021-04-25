using NBomber.Contracts;
using Serilog;
using System.Threading.Tasks;

namespace LoadTesting
{
    public class Subscriber : TokenClientBase
    {
        private string lastOrderText;

        public Subscriber(string endpoint, string secretKey) : base(endpoint, secretKey)
        {
        }

        protected override async Task<Response> OnExecuteAsync()
        {
            var res = await Client.GetAsync("/api/orders");
            if (res.IsSuccessStatusCode)
            {
                Response.Fail($"Order response is not valid. Status code = {res.StatusCode}");
            }
            var orderText = await res.Content.ReadAsStringAsync();

            if (lastOrderText != orderText)
            {
                Log.Logger.Information("Order changed: {0}", orderText);
            }
            lastOrderText = orderText;
            return Response.Ok();
        }
    }
}
