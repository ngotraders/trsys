using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class InMemoryOrderDatabase : InMemoryDatabaseBase<OrderDto, string>, IOrderDatabase
    {
        private OrdersTextEntry Entry = OrdersTextEntry.Create(new List<PublishedOrder>());

        private readonly Dictionary<int, OrderDto> ByTicketNo = [];
        private readonly Dictionary<Guid, List<OrderDto>> BySecretKey = [];
        private List<PublishedOrder> List => All.Select(o => o.Order).ToList();

        public Task AddAsync(OrderDto order)
        {
            return AddAsync(order.Id, order, (item) =>
            {
                if (ByTicketNo.TryAdd(order.TicketNo, order))
                {
                    if (!BySecretKey.TryGetValue(order.SecretKeyId, out var list))
                    {
                        list = new();
                        BySecretKey.Add(order.SecretKeyId, list);
                    }
                    Entry = OrdersTextEntry.Create(List);
                }
            });

        }

        public Task RemoveAsync(string id)
        {
            return RemoveAsync(id, item =>
            {
                ByTicketNo.Remove(item.TicketNo);
                if (BySecretKey.TryGetValue(item.SecretKeyId, out var list))
                {
                    list.Remove(item);
                }
                Entry = OrdersTextEntry.Create(List);
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
                }
            });
        }

        public Task<OrdersTextEntry> FindEntryAsync()
        {
            return Task.FromResult(Entry);
        }

        public Task<List<PublishedOrder>> SearchPublishedOrderAsync()
        {
            return Task.FromResult(List);
        }

        protected override object GetItemValue(OrderDto item, string sortKey)
        {
            throw new NotImplementedException();
        }
    }
}
