using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class OrdersClearCommand : IRequest, IRetryableRequest
    {
        public OrdersClearCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
