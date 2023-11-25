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
        private readonly Dictionary<string, OrderHistoryDto> ByPublisherTicketNo = new();

        public Task AddAsync(OrderHistoryDto order)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryAdd(order.Id, order))
                {
                    All.Add(order);
                    ByPublisherTicketNo[order.TicketNo.ToString()] = order;
                }
            });

        }

        public Task UpdateAsync(OrderHistoryDto order)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(order.Id, out var item))
                {
                    ById.Remove(item.Id);
                    All.Remove(item);
                    if (ById.TryAdd(order.Id, order))
                    {
                        All.Add(order);
                    }
                    return;
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
                    if (ByPublisherTicketNo[item.TicketNo.ToString()] == item)
                    {
                        ByPublisherTicketNo.Remove(item.TicketNo.ToString());
                    }
                }
            });
        }

        public Task<OrderHistoryDto> FindByIdAsync(string id)
        {
            if (ById.TryGetValue(id, out var item))
            {
                return Task.FromResult(Copy(item));
            }
            return Task.FromResult<OrderHistoryDto>(null);
        }

        public Task<OrderHistoryDto> FindByPublisherTicketNoAsync(string ticketNo)
        {
            if (ByPublisherTicketNo.TryGetValue(ticketNo, out var item))
            {
                return Task.FromResult(Copy(item));
            }
            return Task.FromResult<OrderHistoryDto>(null);
        }

        public Task<List<OrderHistoryDto>> SearchAsync()
        {
            return Task.FromResult(All.Select(Copy).ToList());
        }

        private OrderHistoryDto Copy(OrderHistoryDto item)
        {
            var copied = new OrderHistoryDto()
            {
                Id = item.Id,
                PublisherId = item.PublisherId,
                TicketNo = item.TicketNo,
                Symbol = item.Symbol,
                OrderType = item.OrderType,
                PriceOpened = item.PriceOpened,
                TimeOpened = item.TimeOpened,
                PriceClosed = item.PriceClosed,
                TimeClosed = item.TimeClosed,
                Percentage = item.Percentage,
                OpenPublishedAt = item.OpenPublishedAt,
                ClosePublishedAt = item.ClosePublishedAt,
            };
            copied.SubscriberOrderHistories.AddRange(item.SubscriberOrderHistories.Select(x => new SubscriberOrderHistoryDto()
            {
                SubscriberId = x.SubscriberId,
                OpenDeliveredAt = x.OpenDeliveredAt,
                ClosedDeliveredAt = x.ClosedDeliveredAt,
                TicketNo = x.TicketNo,
                Lots = x.Lots,
                PriceOpened = x.PriceOpened,
                TimeOpened = x.TimeOpened,
                PriceClosed = x.PriceClosed,
                TimeClosed = x.TimeClosed,
            }));
            return copied;
        }

        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
