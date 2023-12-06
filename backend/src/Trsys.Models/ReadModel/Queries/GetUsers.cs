using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetUsers : IRequest<SearchResponseDto<UserDto>>
    {
        public GetUsers()
        {
        }

        public GetUsers(int? page, int? perPage)
        {
            Page = page;
            PerPage = perPage;
        }

        public int? Page { get; set; }
        public int? PerPage { get; set; }
    }
}
