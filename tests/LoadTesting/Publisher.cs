using NBomber.Contracts;
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
                res.EnsureSuccessStatusCode();
                sentOrder = orderText;
                return Response.Ok(payload: "Order posted" + sentOrder);
            }
            return Response.Ok(payload: "not requested");
        }
    }
}
