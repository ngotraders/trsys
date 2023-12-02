using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
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
