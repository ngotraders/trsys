using MediatR;
using System.Collections.Generic;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Services.Events
{
    public class OrderUpdated: INotification
    {
        public IEnumerable<Order> Orders { get; }

        public OrderUpdated(IEnumerable<Order> orders)
        {
            Orders = orders;
        }
    }
}
