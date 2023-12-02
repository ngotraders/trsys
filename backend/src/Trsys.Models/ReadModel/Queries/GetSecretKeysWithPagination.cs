using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetSecretKeysWithPagination : IRequest<PagedResultDto<SecretKeyDto>>
    {
        public GetSecretKeysWithPagination(int page = 1, int perPage = 0)
        {
            Page = page;
            PerPage = perPage;
        }

        public int Page { get; }
        public int PerPage { get; }
    }
}
