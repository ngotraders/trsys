using System.Collections.Concurrent;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.InMemory
{
    public class InMemorySecretKeyUsageStore : ISecretKeyUsageStore
    {
        private readonly ConcurrentDictionary<string, SecretKeyUsage> store = new ConcurrentDictionary<string, SecretKeyUsage>();

        public void Add(string key)
        {
            store.TryAdd(key, new SecretKeyUsage()
            {
                SecretKey = key,
            });
        }

        public SecretKeyUsage Find(string key)
        {
            store.TryGetValue(key, out var value);
            return value;
        }

        public void Remove(string key)
        {
            store.TryRemove(key, out _);
        }

        public void Touch(string key)
        {
            if (store.TryGetValue(key, out var value))
            {
                value.Touch();
            }
        }
    }
}
