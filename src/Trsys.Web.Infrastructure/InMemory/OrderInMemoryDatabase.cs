using SqlStreamStore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Infrastructure;

namespace Trsys.Web.Infrastructure.InMemory
{
    public class OrderInMemoryDatabase : IOrderDatabase
    {
        private readonly TaskQueue queue = new();
        public OrdersTextEntry Entry = OrdersTextEntry.Create(new List<PublishedOrder>());

        public readonly List<OrderDto> All = new();
        public readonly Dictionary<string, OrderDto> ById = new();
        public readonly Dictionary<int, OrderDto> ByTicketNo = new();
        public readonly Dictionary<Guid, List<OrderDto>> BySecretKey = new();
        public List<PublishedOrder> List => All.Select(o => o.Order).ToList();

        public Task AddAsync(OrderDto order)
        {
            return queue.Enqueue(() =>
            {
                if (ByTicketNo.TryAdd(order.Order.TicketNo, order))
                {
                    All.Add(order);
                    ById.Add(order.Id, order);
                    if (!BySecretKey.TryGetValue(order.SecretKeyId, out var list))
                    {
                        list = new();
                        BySecretKey.Add(order.SecretKeyId, list);
                    }
                    list.Add(order);
                    Entry = OrdersTextEntry.Create(List);
                }
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
                    ByTicketNo.Remove(item.Order.TicketNo);
                    if (BySecretKey.TryGetValue(item.SecretKeyId, out var list))
                    {
                        list.Remove(item);
                    }
                    Entry = OrdersTextEntry.Create(List);
                }
            });
        }

        public Task RemoveBySecretKeyAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                if (BySecretKey.TryGetValue(id, out var list))
                {
                    BySecretKey.Remove(id);
                    foreach (var item in list.ToList())
                    {
                        ById.Remove(item.Id);
                        All.Remove(item);
                        ByTicketNo.Remove(item.Order.TicketNo);
                    }
                    Entry = OrdersTextEntry.Create(List);
                }
            });
        }

        public Task<OrdersTextEntry> FindEntryAsync()
        {
            return Task.FromResult(Entry);
        }

        public Task<List<OrderDto>> SearchAsync()
        {
            return Task.FromResult(All);
        }

        public Task<List<PublishedOrder>> SearchPublishedOrderAsync()
        {
            return Task.FromResult(List);
        }
    }
}
