using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure.Queue;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class InMemoryOrderHistoryDatabase : IOrderHistoryDatabase, IDisposable
    {
        private readonly BlockingTaskQueue queue = new();

        private readonly List<OrderHistoryDto> All = new();
        private readonly Dictionary<string, OrderHistoryDto> ById = new();
        private readonly Dictionary<int, OrderHistoryDto> ByPublisherTicketNo = new();

        public Task AddAsync(OrderHistoryDto order)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryAdd(order.Id, order))
                {
                    All.Add(order);
                    ByPublisherTicketNo[order.TicketNo] = order;
                    while (All.Count > 10)
                    {
                        All.RemoveAt(0);
                    }
                }
            });
        }

        public Task<OrderHistoryDto> UpdateClosePublishedAtAsync(string id, DateTimeOffset closePublishedAt)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
                {
                    item.ClosePublishedAt = closePublishedAt;
                    return item;
                }
                throw new InvalidOperationException();
            });
        }

        public Task<OrderHistoryDto> AddSubscriberOrderHistoryAsync(string id, string subscriberId, DateTimeOffset openDeliveredAt)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
                {
                    item.SubscriberOrderHistories.Add(new SubscriberOrderHistoryDto()
                    {
                        SubscriberId = subscriberId,
                        OpenDeliveredAt = openDeliveredAt
                    });
                    return item;
                }
                throw new InvalidOperationException();
            });
        }

        public Task<OrderHistoryDto> UpdateSubscriberOrderHistoryClosedDeliveredAtAsync(string id, string subscriberId, DateTimeOffset closeDeliveredAt)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
                {
                    var subscriber = item.SubscriberOrderHistories.FirstOrDefault(s => s.SubscriberId == subscriberId);
                    if (subscriber != null)
                    {
                        subscriber.ClosedDeliveredAt = closeDeliveredAt;
                    }
                    return item;
                }
                throw new InvalidOperationException();
            });
        }

        public Task<OrderHistoryDto> UpdateSubscriberOrderOpenInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceOpened, decimal lotsOpened, DateTimeOffset timeOpened)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
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
                    return item;
                }
                throw new InvalidOperationException();
            });
        }

        public Task<OrderHistoryDto> UpdateSubscriberOrderCloseInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceClosed, decimal lotsClosed, DateTimeOffset timeClosed, decimal profit)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
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
                    return item;
                }
                throw new InvalidOperationException();
            });
        }

        public Task RemoveAsync(string id)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
                {
                    ById.Remove(item.Id);
                    All.Remove(item);
                    if (ByPublisherTicketNo[item.TicketNo] == item)
                    {
                        ByPublisherTicketNo.Remove(item.TicketNo);
                    }
                }
            });
        }

        public Task<OrderHistoryDto> FindByIdAsync(string id)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
                {
                    return item;
                }
                return null;
            });
        }

        public Task<OrderHistoryDto> FindByPublisherTicketNoAsync(int ticketNo)
        {
            return queue.Enqueue(() =>
            {
                if (ByPublisherTicketNo.TryGetValue(ticketNo, out var item))
                {
                    return item;
                }
                return null;
            });
        }

        public Task<List<OrderHistoryDto>> SearchAsync()
        {
            return queue.Enqueue(() =>
            {
                return All.ToList();
            });
        }

        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
