using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class SearchEvents : IRequest<SearchResponseDto<EventDto>>
    {
        public SearchEvents()
        {
        }

        public SearchEvents(int? start, int? end, string source)
        {
            Start = start;
            End = end;
            Source = source;
        }

        public int? Start { get; set; }
        public int? End { get; set; }

        public string Source { get; set; }
    }
}
