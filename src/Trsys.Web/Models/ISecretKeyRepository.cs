using System.Threading.Tasks;

namespace Trsys.Web.Models
{
    public interface ISecretKeyRepository
    {
        Task<SecretKey> CreateNewSecretKeyAsync(SecretKeyType keyType);
        Task<SecretKey> FindBySecretKeyAsync(string secretKey);
        Task SaveAsync(SecretKey result);
    }
}
