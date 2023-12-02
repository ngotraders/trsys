using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class PublisherClearOrdersCommand : IRequest, IRetryableRequest
    {
        public PublisherClearOrdersCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
