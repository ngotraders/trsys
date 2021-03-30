using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Authentication
{
    public interface ISecretTokenStore
    {
        Task<string> RegisterTokenAsync(string secretKey, SecretKeyType keyType);
        Task<SecretTokenInfo> FindInfoAsync(string token);
        Task<SecretTokenInfo> FindInfoUpdatingAccessTimeAsync(string token);
        Task UnregisterAsync(string token);
    }
}
