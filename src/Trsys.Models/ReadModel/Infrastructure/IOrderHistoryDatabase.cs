using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface IOrderHistoryDatabase
    {
        Task AddAsync(OrderHistoryDto order);
        Task UpdateAsync(OrderHistoryDto order);
        Task RemoveAsync(string id);
        Task<OrderHistoryDto> FindByIdAsync(string id);
        Task<OrderHistoryDto> FindByPublisherTicketNoAsync(string ticketNo);
        Task<List<OrderHistoryDto>> SearchAsync();
    }
}
