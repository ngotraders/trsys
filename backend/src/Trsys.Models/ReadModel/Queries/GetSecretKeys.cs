using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetSecretKeys : IRequest<SearchResponseDto<SecretKeyDto>>
    {
        public GetSecretKeys()
        {
        }

        public GetSecretKeys(int? start, int? end)
        {
            Start = start;
            End = end;
        }

        public int? Start { get; set; }
        public int? End { get; set; }
    }
}
