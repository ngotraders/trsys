using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Infrastructure
{
    public interface IOrderDatabase
    {
        Task AddAsync(OrderDto order);
        Task RemoveAsync(string id);
        Task RemoveBySecretKeyAsync(Guid id);
        Task<OrdersTextEntry> FindEntryAsync(string version);
        Task<List<OrderDto>> SearchAsync();
        Task<List<PublishedOrder>> SearchPublishedOrderAsync();
    }
}
