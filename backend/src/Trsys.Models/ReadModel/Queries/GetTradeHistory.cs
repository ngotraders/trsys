using MediatR;
using System;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetTradeHistory : IRequest<TradeHistoryDto>
    {
        public GetTradeHistory(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
