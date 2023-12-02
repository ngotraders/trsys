using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class FindByUsername : IRequest<UserDto>
    {
        public FindByUsername(string username)
        {
            Username = username;
        }

        public string Username { get; }
    }
}
