using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.KeyValueStores
{
    public class SecretKeyUsageStore : ISecretKeyUsageStore
    {
        private readonly IKeyValueStore<SecretKeyUsage> store;

        public SecretKeyUsageStore(IKeyValueStoreFactory factory)
        {
            this.store = factory.Create<SecretKeyUsage>("SecretKeyUsage");
        }

        public Task AddAsync(string key)
        {
            return store.PutAsync(key, new SecretKeyUsage()
            {
                SecretKey = key,
            });
        }

        public Task<SecretKeyUsage> FindAsync(string key)
        {
            return store.GetAsync(key);
        }

        public Task RemoveAsync(string key)
        {
            return store.DeleteAsync(key);
        }

        public async Task TouchAsync(string key)
        {
            var usage = await FindAsync(key);
            if (usage != null)
            {
                usage.Touch();
                await store.PutAsync(key, usage);
            }
            else
            {
                usage = new SecretKeyUsage
                {
                    SecretKey = key
                };
                usage.Touch();
                await store.PutAsync(key, usage);
            }
        }
    }
}
