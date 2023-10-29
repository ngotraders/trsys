using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class OrderCloseCommand : IRequest, IRetryableRequest
    {
        public OrderCloseCommand(Guid id, int ticketNo)
        {
            Id = id;
            TicketNo = ticketNo;
        }

        public Guid Id { get; }
        public int TicketNo { get; }
    }
}
