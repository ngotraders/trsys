using NBomber.Contracts;
using System.Threading.Tasks;

namespace LoadTesting
{
    public class Subscriber : TokenClientBase
    {
        private readonly OrderProvider orderProvider;

        public Subscriber(string endpoint, string secretKey, OrderProvider orderProvider) : base(endpoint, secretKey)
        {
            this.orderProvider = orderProvider;
        }

        protected override async Task<Response> OnExecuteAsync()
        {
            var orderTextPrev = orderProvider.GetCurrentOrder();
            var res = await Client.GetAsync("/api/orders");
            res.EnsureSuccessStatusCode();
            var orderResponse = await res.Content.ReadAsStringAsync();

            var orderText = orderProvider.GetCurrentOrder();
            if (orderResponse != orderTextPrev && orderResponse != orderText)
            {
                Response.Fail($"Order response is not valid :{orderResponse}");
            }
            return Response.Ok();
        }
    }
}
