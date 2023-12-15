using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class FindByNormalizedUsername : IRequest<UserDto>
    {
        public FindByNormalizedUsername(string username)
        {
            Username = username;
        }

        public string Username { get; }
    }
}
