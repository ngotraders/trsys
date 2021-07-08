using MediatR;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Queries
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
