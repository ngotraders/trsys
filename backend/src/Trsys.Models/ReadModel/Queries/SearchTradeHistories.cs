using MediatR;
using System.Collections.Generic;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class SearchTradeHistories : IRequest<SearchResponseDto<TradeHistoryDto>>
    {
        public SearchTradeHistories(int? start, int? end, string[] sort, string[] order)
        {
            Start = start;
            End = end;
            Sort = sort;
            Order = order;
        }

        public int? Start { get; }
        public int? End { get; }
        public string[] Sort { get; }
        public string[] Order { get; }
    }
}
