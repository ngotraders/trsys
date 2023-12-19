using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure.Queue;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class InMemoryTradeHistoryDatabase : ITradeHistoryDatabase, IDisposable
    {
        private readonly BlockingTaskQueue queue = new();

        private readonly List<TradeHistoryDto> All = new();
        private readonly Dictionary<string, TradeHistoryDto> ById = new();
        private readonly Dictionary<int, TradeHistoryDto> ByPublisherTicketNo = new();

        public Task AddAsync(TradeHistoryDto order)
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

        public Task<TradeHistoryDto> UpdateClosePublishedAtAsync(string id, DateTimeOffset closePublishedAt)
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

        public Task<TradeHistoryDto> AddSubscriberTradeHistoryAsync(string id, string subscriberId, DateTimeOffset openDeliveredAt)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
                {
                    item.SubscriberOrderHistories.Add(new SubscriberTradeHistoryDto()
                    {
                        SubscriberId = subscriberId,
                        OpenDeliveredAt = openDeliveredAt
                    });
                    return item;
                }
                throw new InvalidOperationException();
            });
        }

        public Task<TradeHistoryDto> UpdateSubscriberTradeHistoryClosedDeliveredAtAsync(string id, string subscriberId, DateTimeOffset closeDeliveredAt)
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

        public Task<TradeHistoryDto> UpdateSubscriberOrderOpenInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceOpened, decimal lotsOpened, DateTimeOffset timeOpened)
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

        public Task<TradeHistoryDto> UpdateSubscriberOrderCloseInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceClosed, decimal lotsClosed, DateTimeOffset timeClosed, decimal profit)
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

        public Task<TradeHistoryDto> FindByIdAsync(string id)
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

        public Task<TradeHistoryDto> FindByPublisherTicketNoAsync(int ticketNo)
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

        public Task<int> CountAsync()
        {
            return queue.Enqueue(() =>
            {
                return All.Count;
            });
        }

        public Task<List<TradeHistoryDto>> SearchAsync()
        {
            return queue.Enqueue(() =>
            {
                return All.ToList();
            });
        }

        public Task<List<TradeHistoryDto>> SearchAsync(int start, int end, string[] sort, string[] order)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (end <= start)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }
            return queue.Enqueue(() =>
            {
                var query = All as IEnumerable<TradeHistoryDto>;
                if (sort != null && order != null)
                {
                    for (var i = 0; i < sort.Length; i++)
                    {
                        var sortKey = sort[i];
                        var orderKey = order[i];
                        if (orderKey == "asc")
                        {
                            query = query.OrderBy(item => GetItemValue(item, sortKey));
                        }
                        else if (orderKey == "desc")
                        {
                            query = query.OrderByDescending(item => GetItemValue(item, sortKey));
                        }
                    }
                }
                return query.Skip(start).Take(end - start).ToList();
            });
        }

        private static object GetItemValue(TradeHistoryDto item, string sortKey)
        {
            return sortKey switch
            {
                "id" => item.Id,
                "publisherId" => item.PublisherId,
                "ticketNo" => item.TicketNo,
                "openPublishedAt" => item.OpenPublishedAt,
                "closePublishedAt" => item.ClosePublishedAt,
                _ => throw new InvalidOperationException(),
            };
        }

        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
