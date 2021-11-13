using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Queue;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Infrastructure;

namespace Trsys.Web.Infrastructure.ReadModel.InMemory
{
    public class InMemoryOrderDatabase : IOrderDatabase, IDisposable
    {
        private readonly BlockingTaskQueue queue = new();
        private OrdersTextEntry Entry = OrdersTextEntry.Create(new List<PublishedOrder>());
        private OrdersTextEntry EntryV2 = OrdersTextEntry.CreateV2(new List<PublishedOrder>());

        private readonly List<OrderDto> All = new();
        private readonly Dictionary<string, OrderDto> ById = new();
        private readonly Dictionary<int, OrderDto> ByTicketNo = new();
        private readonly Dictionary<Guid, List<OrderDto>> BySecretKey = new();
        private List<PublishedOrder> List => All.Select(o => o.Order).ToList();

        public Task AddAsync(OrderDto order)
        {
            return queue.Enqueue(() =>
            {
                if (ByTicketNo.TryAdd(order.TicketNo, order))
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
                    EntryV2 = OrdersTextEntry.CreateV2(List);
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
                    ByTicketNo.Remove(item.TicketNo);
                    if (BySecretKey.TryGetValue(item.SecretKeyId, out var list))
                    {
                        list.Remove(item);
                    }
                    Entry = OrdersTextEntry.Create(List);
                    EntryV2 = OrdersTextEntry.CreateV2(List);
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
                        ByTicketNo.Remove(item.TicketNo);
                    }
                    Entry = OrdersTextEntry.Create(List);
                    EntryV2 = OrdersTextEntry.CreateV2(List);
                }
            });
        }

        public Task<OrdersTextEntry> FindEntryAsync(string version)
        {
            switch (version)
            {
                case "v1":
                    return Task.FromResult(Entry);
                case "v2":
                    return Task.FromResult(EntryV2); 
                default:
                    throw new ArgumentException(null, nameof(version));
            }
        }

        public Task<List<OrderDto>> SearchAsync()
        {
            return Task.FromResult(All);
        }

        public Task<List<PublishedOrder>> SearchPublishedOrderAsync()
        {
            return Task.FromResult(List);
        }
        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
