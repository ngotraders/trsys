using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetUsers : IRequest<SearchResponseDto<UserDto>>
    {
        public GetUsers()
        {
        }

        public GetUsers(int? start, int? end)
        {
            Start = start;
            End = end;
        }

        public int? Start { get; set; }
        public int? End { get; set; }
    }
}
