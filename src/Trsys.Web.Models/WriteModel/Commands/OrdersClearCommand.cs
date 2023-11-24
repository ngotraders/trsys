using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
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
