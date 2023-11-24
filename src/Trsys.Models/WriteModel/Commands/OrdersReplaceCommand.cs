using MediatR;
using System;
using System.Collections.Generic;

namespace Trsys.Models.WriteModel.Commands
{
    public class OrdersReplaceCommand : IRequest, IRetryableRequest
    {
        public OrdersReplaceCommand(Guid id, IEnumerable<PublishedOrder> publishedOrders)
        {
            Id = id;
            PublishedOrders = publishedOrders;
        }

        public Guid Id { get; }
        public IEnumerable<PublishedOrder> PublishedOrders { get; }
    }
}
