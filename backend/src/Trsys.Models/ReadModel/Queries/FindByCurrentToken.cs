using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
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
