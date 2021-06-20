using MediatR;
using System.Collections.Generic;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Queries
{
    public class GetEvents : IRequest<IEnumerable<EventDto>>
    {
        public GetEvents() : this(null, 0, 0)
        {
        }

        public GetEvents(string source, int page, int perPage)
        {
            Source = source;
            Page = page;
            PerPage = perPage;
        }

        public string Source { get; set; }
        public int PerPage { get; set; }
        public int Page { get; set; }
    }
}
