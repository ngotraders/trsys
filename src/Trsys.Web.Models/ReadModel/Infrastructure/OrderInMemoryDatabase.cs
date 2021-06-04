using System;
using System.Collections.Generic;
using System.Linq;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Infrastructure
{
    public class OrderInMemoryDatabase
    {
        public OrdersTextEntry Entry = OrdersTextEntry.Create(new List<PublishedOrder>());

        public readonly List<OrderDto> All = new();
        public readonly Dictionary<string, OrderDto> ById = new();
        public readonly Dictionary<int, OrderDto> ByTicketNo = new();
        public readonly Dictionary<Guid, List<OrderDto>> BySecretKey = new();
        public List<PublishedOrder> List => All.Select(o => o.Order).ToList();

        public void Add(OrderDto order)
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
        }

        public void Remove(string id)
        {
            var item = ById[id];
            ById.Remove(item.Id);
            All.RemoveAt(All.IndexOf(item));
            ByTicketNo.Remove(item.Order.TicketNo);
            BySecretKey.Remove(item.SecretKeyId);
        }

        public void RemoveBySecretKey(Guid id)
        {
            if (BySecretKey.TryGetValue(id, out var list))
            {
                foreach (var item in list)
                {
                    Remove(item.Id);
                }
            }
        }
    }
}
