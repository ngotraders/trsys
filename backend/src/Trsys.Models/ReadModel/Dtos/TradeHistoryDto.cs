using System;
using System.Collections.Generic;

namespace Trsys.Models.ReadModel.Dtos
{
    public class TradeHistoryDto
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

        public List<SubscriberTradeHistoryDto> SubscriberOrderHistories { get; } = new();
    }

    public class SubscriberTradeHistoryDto
    {
        public string SubscriberId { get; set; }
        public DateTimeOffset OpenDeliveredAt { get; set; }
        public DateTimeOffset? ClosedDeliveredAt { get; set; }

        public int? TicketNo { get; set; }
        public int? TradeNo { get; set; }
        public decimal? PriceOpened { get; set; }
        public decimal? LotsOpened { get; set; }
        public DateTimeOffset? TimeOpened { get; set; }
        public decimal? PriceClosed { get; set; }
        public decimal? LotsClosed { get; set; }
        public DateTimeOffset? TimeClosed { get; set; }
        public decimal? Profit { get; set; }
    }
}
