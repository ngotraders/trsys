using MediatR;
using System.Collections.Generic;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class SearchOrders : IRequest<List<OrderDto>>
    {
    }
}
