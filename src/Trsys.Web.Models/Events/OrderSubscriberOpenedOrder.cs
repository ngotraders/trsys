using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.Events
{
    public class OrderSubscriberOpenedOrder : INotification, IEvent
    {
        public OrderSubscriberOpenedOrder(Guid id, int ticketNo)
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