using MediatR;
using System.Collections.Generic;

namespace Trsys.Web.Models.ReadModel.Queries
{
    public class GetEvents : IRequest<List<Event>>
    {
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
