using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class SearchUsers : IRequest<SearchResponseDto<UserDto>>
    {
        public SearchUsers()
        {
        }

        public SearchUsers(int? start, int? end, string[] sort, string[] order)
        {
            Start = start;
            End = end;
            Sort = sort;
            Order = order;
        }

        public int? Start { get; set; }
        public int? End { get; set; }
        public string[] Sort { get; set; }
        public string[] Order { get; set; }
    }
}
