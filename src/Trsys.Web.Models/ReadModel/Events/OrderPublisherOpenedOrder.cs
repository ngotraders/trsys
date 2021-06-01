using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class OrderPublisherOpenedOrder : INotification, IEvent
    {
        public OrderPublisherOpenedOrder(Guid id, PublishedOrder order)
        {
            Id = id;
            Order = order;
        }

        public Guid Id { get; set; }
        public PublishedOrder Order { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}