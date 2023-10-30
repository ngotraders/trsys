using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface IOrderDatabase
    {
        Task AddAsync(OrderDto order);
        Task RemoveAsync(string id);
        Task RemoveBySecretKeyAsync(Guid id);
        Task<OrdersTextEntry> FindEntryAsync();
        Task<List<OrderDto>> SearchAsync();
        Task<List<PublishedOrder>> SearchPublishedOrderAsync();
    }
}
