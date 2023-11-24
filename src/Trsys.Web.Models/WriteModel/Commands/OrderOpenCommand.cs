using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class OrderOpenCommand : IRequest, IRetryableRequest
    {
        public OrderOpenCommand(Guid id, PublishedOrder publishedOrder)
        {
            Id = id;
            PublishedOrder = publishedOrder;
        }

        public Guid Id { get; }
        public PublishedOrder PublishedOrder { get; }
    }
}
