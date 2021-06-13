using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Infrastructure;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Models.ReadModel.Handlers
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
