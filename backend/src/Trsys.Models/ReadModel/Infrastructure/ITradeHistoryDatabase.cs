using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface ITradeHistoryDatabase
    {
        Task AddAsync(TradeHistoryDto order);
        Task<TradeHistoryDto> UpdateClosePublishedAtAsync(string id, DateTimeOffset closePublishedAt);
        Task<TradeHistoryDto> AddSubscriberTradeHistoryAsync(string id, string subscriberId, DateTimeOffset openDeliveredAt);
        Task<TradeHistoryDto> UpdateSubscriberTradeHistoryClosedDeliveredAtAsync(string id, string subscriberId, DateTimeOffset closeDeliveredAt);
        Task<TradeHistoryDto> UpdateSubscriberOrderOpenInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceOpened, decimal lotsOpened, DateTimeOffset timeOpened);
        Task<TradeHistoryDto> UpdateSubscriberOrderCloseInfoAsync(string id, string subscriberId, int ticketNo, int tradeNo, decimal priceClosed, decimal lotsClosed, DateTimeOffset timeClosed, decimal profit);
        Task RemoveAsync(string id);
        Task<TradeHistoryDto> FindByIdAsync(string id);
        Task<TradeHistoryDto> FindByPublisherTicketNoAsync(int ticketNo);
        Task<int> CountAsync();
        Task<List<TradeHistoryDto>> SearchAsync();
        Task<List<TradeHistoryDto>> SearchAsync(int start, int end, string[] sort, string[] order);
    }
}
