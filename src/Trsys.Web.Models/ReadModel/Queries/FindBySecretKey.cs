using MediatR;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Queries
{
    public class FindBySecretKey : IRequest<SecretKeyDto>
    {
        public FindBySecretKey(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}
