﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trsys.Web.Models
{
    public interface IOrderRepository
    {
        Task SaveOrdersAsync(IEnumerable<Order> orders);
        Task<List<Order>> SearchAllAsync();
    }
}
