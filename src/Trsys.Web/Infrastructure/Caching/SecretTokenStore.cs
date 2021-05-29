using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.Caching
{
    public class SecretTokenStore : ISecretTokenStore
    {
        private readonly IKeyValueStore<SecretToken> store;

        public SecretTokenStore(IKeyValueStoreFactory factory)
        {
            this.store = factory.Create<SecretToken>("SecretToken");
        }

        public Task AddAsync(string secretKey, SecretKeyType keyType, string token)
        {
            return store.PutAsync(token, new SecretToken()
            {
                Token = token,
                Key = secretKey,
                KeyType = keyType,
            });
        }

        public Task<SecretToken> FindAsync(string token)
        {
            return store.GetAsync(token);
        }

        public Task RemoveAsync(string token)
        {
            return store.DeleteAsync(token);
        }
    }
}
