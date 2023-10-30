using MediatR;
using System.Collections.Generic;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetLogs : IRequest<IEnumerable<LogDto>>
    {
        public GetLogs() : this(null, 0, 0)
        {
        }

        public GetLogs(string source, int page, int perPage)
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
