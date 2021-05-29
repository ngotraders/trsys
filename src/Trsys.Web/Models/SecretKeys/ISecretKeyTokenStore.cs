using System.Threading.Tasks;

namespace Trsys.Web.Models.SecretKeys
{
    public interface ISecretKeyTokenStore
    {
        Task<SecretKeyToken> FindAsync(string key);
        Task SaveAsync(SecretKeyToken secretKey);
        Task RemoveAsync(string key);
    }
}
