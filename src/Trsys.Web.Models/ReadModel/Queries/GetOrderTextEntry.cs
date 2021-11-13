using MediatR;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Queries
{
    public class GetOrderTextEntry : IRequest<OrdersTextEntry>
    {
        public GetOrderTextEntry(string version)
        {
            Version = version;
        }

        public string Version { get; }
    }
}