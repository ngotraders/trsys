using NBomber.Contracts;
using Serilog;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LoadTesting
{
    public class Publisher : TokenClientBase
    {
        private readonly OrderProvider orderProvider;
        private string sentOrder;

        public Publisher(string endpoint, string secretKey, OrderProvider orderProvider) : base(endpoint, secretKey)
        {
            this.orderProvider = orderProvider;
        }

        protected override async Task<Response> OnExecuteAsync()
        {
            var orderText = orderProvider.GetCurrentOrder();
            if (sentOrder != orderText)
            {
                var res = await Client.PostAsync("/api/orders", new StringContent(orderText, Encoding.UTF8, "text/plain"));
                if (!res.IsSuccessStatusCode)
                {
                    Response.Fail($"Order response is not valid. Status code = {res.StatusCode}");
                }
                Log.Logger.Information($"Publisher:{SecretKey}:OrderUpdated:{orderText}");
                sentOrder = orderText;
                return Response.Ok(payload: "Order posted" + sentOrder);
            }
            return Response.Ok(payload: "not requested");
        }
    }
}
