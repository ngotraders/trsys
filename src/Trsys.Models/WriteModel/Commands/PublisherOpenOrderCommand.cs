using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class PublisherOpenOrderCommand : IRequest, IRetryableRequest
    {
        public PublisherOpenOrderCommand(Guid id, PublishedOrder publishedOrder)
        {
            Id = id;
            PublishedOrder = publishedOrder;
        }

        public Guid Id { get; }
        public PublishedOrder PublishedOrder { get; }
    }
}
