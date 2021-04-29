using System.Collections.Concurrent;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.InMemory
{
    public class InMemorySecretKeyUsageStore : ISecretKeyUsageStore
    {
        private readonly ConcurrentDictionary<string, SecretKeyUsage> store = new ConcurrentDictionary<string, SecretKeyUsage>();

        public Task AddAsync(string key)
        {
            store.TryAdd(key, new SecretKeyUsage()
            {
                SecretKey = key,
            });
            return Task.CompletedTask;
        }

        public Task<SecretKeyUsage> FindAsync(string key)
        {
            store.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public Task RemoveAsync(string key)
        {
            store.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task TouchAsync(string key)
        {
            if (store.TryGetValue(key, out var value))
            {
                value.Touch();
            }
            return Task.CompletedTask;
        }
    }
}
