using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class OrderSubscriberClosedOrder : INotification, IEvent
    {
        public OrderSubscriberClosedOrder(Guid id, int ticketNo)
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