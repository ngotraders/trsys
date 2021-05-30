using MediatR;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Queries
{
    public class FindByCurrentToken : IRequest<SecretKeyDto>
    {
        public FindByCurrentToken(string token)
        {
            Token = token;
        }

        public string Token { get; }
    }
}
