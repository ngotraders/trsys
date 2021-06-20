using MediatR;
using System;
using System.Collections.Generic;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class PublishOrderCommand : IRequest, IRetryableRequest
    {
        public PublishOrderCommand(Guid id, IEnumerable<PublishedOrder> publishedOrders)
        {
            Id = id;
            PublishedOrders = publishedOrders;
        }

        public Guid Id { get; }
        public IEnumerable<PublishedOrder> PublishedOrders { get; }
    }
}
