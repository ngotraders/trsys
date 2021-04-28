using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trsys.Web.Models.SecretKeys
{
    public interface ISecretKeyRepository
    {
        Task<SecretKey> CreateNewSecretKeyAsync(SecretKeyType keyType);
        Task<SecretKey> FindBySecretKeyAsync(string secretKey);
        Task<List<SecretKey>> SearchAllAsync();
        Task SaveAsync(SecretKey entity);
        Task RemoveAsync(SecretKey entity);
    }
}
