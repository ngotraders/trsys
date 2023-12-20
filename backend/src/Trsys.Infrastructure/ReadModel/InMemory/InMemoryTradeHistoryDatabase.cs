using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class InMemoryTradeHistoryDatabase : InMemoryDatabaseBase<TradeHistoryDto, string>, ITradeHistoryDatabase
    {
        private readonly Dictionary<int, TradeHistoryDto> ByPublisherTicketNo = [];

        public Task AddAsync(TradeHistoryDto order)
        {
            return AddAsync(order.Id, order, _ =>
            {
                ByPublisherTicketNo[order.TicketNo] = order;
                while (All.Count > 10)
                {
                    All.RemoveAt(0);
                }
            });
        }

        public Task<TradeHistoryDto> UpdateClosePublishedAtAsync(string id, DateTimeOffset closePublishedAt)
        {
            return UpdateAsync(id, item =>
            {
                item.ClosePublishedAt = closePublishedAt;
            });
        }

        public Task<TradeHistoryDto> AddSubscriberTradeHistoryAsync(string id, string subscriberId, DateTimeOffset openDeliveredAt)
        {
            return UpdateAsync(id, item =>
            {
                item.SubscriberOrderHistories.Add(new SubscriberTradeHistoryDto()
                {
                    SubscriberId = subscriberId,
                    OpenDeliveredAt = openDeliveredAt
                });
            });
        }

        public Task<TradeHistoryDto> UpdateSubscriberTradeHistoryClosedDeliveredAtAsync(string id, string subscriberId, DateTimeOffset closeDeliveredAt)
        {
            return UpdateAsync(id, item =>
            {
                var subscriber = item.SubscriberOrderHistories.FirstOrDefault(s => s.SubscriberId == subscriberId);
                if (subscriber != null)
                {
                    subscriber.ClosedDeliveredAt = closeDeliveredAt;
                }
            });
        }

        public Task<TradeHistoryDto> UpdateSubscriberOrderOpenInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceOpened, decimal lotsOpened, DateTimeOffset timeOpened)
        {
            return UpdateAsync(id, item =>
            {
                var subscriber = item.SubscriberOrderHistories.FirstOrDefault(s => s.SubscriberId == subscriberId);
                if (subscriber != null)
                {
                    subscriber.TicketNo = ticketNo;
                    subscriber.TradeNo = tradeNo;
                    subscriber.PriceOpened = priceOpened;
                    subscriber.LotsOpened = lotsOpened;
                    subscriber.TimeOpened = timeOpened;
                }
            });
        }

        public Task<TradeHistoryDto> UpdateSubscriberOrderCloseInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceClosed, decimal lotsClosed, DateTimeOffset timeClosed, decimal profit)
        {
            return UpdateAsync(id, item =>
            {
                var subscriber = item.SubscriberOrderHistories.FirstOrDefault(s => s.SubscriberId == subscriberId);
                if (subscriber != null)
                {
                    subscriber.TicketNo = ticketNo;
                    subscriber.TradeNo = tradeNo;
                    subscriber.PriceClosed = priceClosed;
                    subscriber.LotsClosed = lotsClosed;
                    subscriber.TimeClosed = timeClosed;
                }
            });
        }

        public Task RemoveAsync(string id)
        {
            return RemoveAsync(id, item =>
            {
                if (ByPublisherTicketNo[item.TicketNo] == item)
                {
                    ByPublisherTicketNo.Remove(item.TicketNo);
                }
            });
        }

        public Task<TradeHistoryDto> FindByPublisherTicketNoAsync(int ticketNo)
        {
            if (ByPublisherTicketNo.TryGetValue(ticketNo, out var item))
            {
                return Task.FromResult(item);
            }
            return Task.FromResult(null as TradeHistoryDto);
        }

        protected override object GetItemValue(TradeHistoryDto item, string sortKey)
        {
            return sortKey switch
            {
                "id" => item.Id,
                "publisherId" => item.PublisherId,
                "ticketNo" => item.TicketNo,
                "symbol" => item.Symbol,
                "orderType" => item.OrderType,
                "openPublishedAt" => item.OpenPublishedAt,
                "closePublishedAt" => item.ClosePublishedAt,
                "isOpen" => item.IsOpen,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
