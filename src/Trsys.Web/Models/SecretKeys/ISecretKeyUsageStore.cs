using System.Threading.Tasks;

namespace Trsys.Web.Models.SecretKeys
{
    public interface ISecretKeyUsageStore
    {
        Task AddAsync(string key);
        Task<SecretKeyUsage> FindAsync(string key);
        Task TouchAsync(string key);
        Task RemoveAsync(string key);
    }
}
