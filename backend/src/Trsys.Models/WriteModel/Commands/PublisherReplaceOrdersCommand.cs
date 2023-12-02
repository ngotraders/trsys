using MediatR;
using System;
using System.Collections.Generic;

namespace Trsys.Models.WriteModel.Commands
{
    public class PublisherReplaceOrdersCommand : IRequest, IRetryableRequest
    {
        public PublisherReplaceOrdersCommand(Guid id, IEnumerable<PublishedOrder> publishedOrders)
        {
            Id = id;
            PublishedOrders = publishedOrders;
        }

        public Guid Id { get; }
        public IEnumerable<PublishedOrder> PublishedOrders { get; }
    }
}
