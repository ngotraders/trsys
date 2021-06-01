using System.Collections.Generic;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Infrastructure
{
    public class OrderInMemoryDatabase
    {
        public OrdersTextEntry Entry = OrdersTextEntry.Create(new List<PublishedOrder>());

        public readonly List<OrderDto> All = new();
        public readonly Dictionary<string, OrderDto> ById = new();
        public readonly List<PublishedOrder> List = new();
        public readonly Dictionary<int, PublishedOrder> ByTicketNo = new();

        public void Add(OrderDto order)
        {
            All.Add(order);
            ById.Add(order.Id, order);
            ByTicketNo.Add(order.Order.TicketNo, order.Order);
            List.Add(order.Order);
            Entry = OrdersTextEntry.Create(List);
        }

        public void Remove(string id)
        {
            var item = ById[id];
            var item2 = ByTicketNo[item.Order.TicketNo];
            ById.Remove(id);
            All.RemoveAt(All.IndexOf(item));
            ByTicketNo.Remove(item.Order.TicketNo);
            List.RemoveAt(List.IndexOf(item2));
            Entry = OrdersTextEntry.Create(List);
        }
    }
}
