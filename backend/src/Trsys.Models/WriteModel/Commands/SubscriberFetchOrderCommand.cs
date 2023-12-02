using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class SubscriberFetchOrderCommand : IRequest, IRetryableRequest
    {
        public SubscriberFetchOrderCommand(Guid id, int[] tickets)
        {
            Id = id;
            Tickets = tickets;
        }

        public Guid Id { get; }
        public int[] Tickets { get; }
    }
}
