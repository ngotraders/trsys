using System.Threading.Tasks;

namespace Trsys.Web.Models.SecretKeys
{
    public interface ISecretTokenStore
    {
        Task<SecretToken> FindAsync(string token);
        Task AddAsync(string key, SecretKeyType keyType, string token);
        Task<bool> VerifyAndTouchAsync(string token, SecretKeyType? keyType = null);
        Task RemoveAsync(string token);
    }
}
