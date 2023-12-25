using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;
using Trsys.Models.ReadModel.Queries;

namespace Trsys.Models.ReadModel.Handlers
{
    public class EventHandlers : IRequestHandler<SearchEvents, SearchResponseDto<EventDto>>
    {
        private readonly IEventDatabase db;

        public EventHandlers(IEventDatabase db)
        {
            this.db = db;
        }

        public async Task<SearchResponseDto<EventDto>> Handle(SearchEvents request, CancellationToken cancellationToken)
        {
            var count = await db.CountAsync(request.Source);
            return new SearchResponseDto<EventDto>(
                count,
                await db.SearchAsync(
                    request.Start ?? 0,
                    request.End ?? int.MaxValue,
                    request.Source));
        }
    }
}
