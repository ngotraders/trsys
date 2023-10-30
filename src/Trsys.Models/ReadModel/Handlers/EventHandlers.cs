using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;
using Trsys.Models.ReadModel.Queries;

namespace Trsys.Models.ReadModel.Handlers
{
    public class EventHandlers : IRequestHandler<GetEvents, IEnumerable<EventDto>>
    {
        private readonly IEventDatabase db;

        public EventHandlers(IEventDatabase db)
        {
            this.db = db;
        }

        public Task<IEnumerable<EventDto>> Handle(GetEvents request, CancellationToken cancellationToken)
        {
            return db.SearchAsync(request.Source, request.Page, request.PerPage);
        }
    }
}
