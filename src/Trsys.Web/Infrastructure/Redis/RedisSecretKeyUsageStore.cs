using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.Redis
{
    public class RedisSecretKeyUsageStore : ISecretKeyUsageStore
    {
        private readonly IDistributedCache cache;

        public RedisSecretKeyUsageStore(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public Task AddAsync(string key)
        {
            return cache.SetObjectAsync(GenerateKey(key), new SecretKeyUsage()
            {
                SecretKey = key,
            });
        }

        public Task<SecretKeyUsage> FindAsync(string key)
        {
            return cache.GetObjectAsync<SecretKeyUsage>(GenerateKey(key));
        }

        public Task RemoveAsync(string key)
        {
            return cache.RemoveAsync(GenerateKey(key));
        }

        public async Task TouchAsync(string key)
        {
            var usage = await FindAsync(key);
            usage.Touch();
            await cache.SetObjectAsync(GenerateKey(key), usage);
        }

        private string GenerateKey(string key)
        {
            return $"SecretKeyUsage/{key}";
        }
    }
}
