using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface IOrderHistoryDatabase
    {
        Task AddAsync(OrderHistoryDto order);
        Task<OrderHistoryDto> UpdateClosePublishedAtAsync(string id, DateTimeOffset closePublishedAt);
        Task<OrderHistoryDto> AddSubscriberOrderHistoryAsync(string id, string subscriberId, DateTimeOffset openDeliveredAt);
        Task<OrderHistoryDto> UpdateSubscriberOrderHistoryClosedDeliveredAtAsync(string id, string subscriberId, DateTimeOffset closeDeliveredAt);
        Task<OrderHistoryDto> UpdateSubscriberOrderOpenInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceOpened, decimal lotsOpened, DateTimeOffset timeOpened);
        Task<OrderHistoryDto> UpdateSubscriberOrderCloseInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceClosed, decimal lotsClosed, DateTimeOffset timeClosed, decimal profit);
        Task RemoveAsync(string id);
        Task<OrderHistoryDto> FindByIdAsync(string id);
        Task<OrderHistoryDto> FindByPublisherTicketNoAsync(int ticketNo);
        Task<List<OrderHistoryDto>> SearchAsync();
    }
}
