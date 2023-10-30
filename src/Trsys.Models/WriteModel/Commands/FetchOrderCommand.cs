using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class FetchOrderCommand : IRequest, IRetryableRequest
    {
        public FetchOrderCommand(Guid id, int[] tickets)
        {
            Id = id;
            Tickets = tickets;
        }

        public Guid Id { get; }
        public int[] Tickets { get; }
    }
}
