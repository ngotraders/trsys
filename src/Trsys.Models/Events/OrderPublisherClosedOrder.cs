using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Models.Events
{
    public class OrderPublisherClosedOrder : INotification, IEvent
    {
        public OrderPublisherClosedOrder(Guid id, int ticketNo)
        {
            Id = id;
            TicketNo = ticketNo;
        }

        public Guid Id { get; set; }
        public int TicketNo { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}