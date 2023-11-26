using System;
using System.Collections.Generic;

namespace Trsys.Models.ReadModel.Dtos
{
    public class OrderHistoryDto
    {
        public string Id { get; set; }
        public Guid PublisherId { get; set; }
        public int TicketNo { get; set; }
        public string Symbol { get; set; }
        public OrderType OrderType { get; set; }
        public decimal PriceOpened { get; set; }
        public DateTimeOffset TimeOpened { get; set; }
        public decimal? PriceClosed { get; set; }
        public DateTimeOffset? TimeClosed { get; set; }
        public decimal Percentage { get; set; }
        public DateTimeOffset OpenPublishedAt { get; set; }
        public DateTimeOffset? ClosePublishedAt { get; set; }

        public List<SubscriberOrderHistoryDto> SubscriberOrderHistories { get; } = new();
    }

    public class SubscriberOrderHistoryDto
    {
        public Guid SubscriberId { get; set; }
        public DateTimeOffset OpenDeliveredAt { get; set; }
        public DateTimeOffset? ClosedDeliveredAt { get; set; }

        public int? TicketNo { get; set; }
        public decimal? Lots { get; set; }
        public decimal? PriceOpened { get; set; }
        public DateTimeOffset? TimeOpened { get; set; }
        public decimal? PriceClosed { get; set; }
        public DateTimeOffset? TimeClosed { get; set; }
        public decimal? Profit { get; set; }
    }
}
