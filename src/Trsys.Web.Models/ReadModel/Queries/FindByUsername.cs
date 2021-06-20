using MediatR;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Queries
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
