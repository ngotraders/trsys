using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class PublisherCloseOrderCommand : IRequest, IRetryableRequest
    {
        public PublisherCloseOrderCommand(Guid id, int ticketNo)
        {
            Id = id;
            TicketNo = ticketNo;
        }

        public Guid Id { get; }
        public int TicketNo { get; }
    }
}
