using MediatR;
using System.Collections.Generic;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetPublishedOrders : IRequest<List<PublishedOrder>>
    {
    }
}
