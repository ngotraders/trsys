using MediatR;
using System.Collections.Generic;

namespace Trsys.Web.Models.ReadModel.Queries
{
    public class GetPublishedOrders : IRequest<List<PublishedOrder>>
    {
    }
}
