using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.KeyValueStores.InMemory
{
    public class InMemoryKeyValueStore<T> : IKeyValueStore<T>
    {
        private readonly ConcurrentDictionary<string, T> store = new ConcurrentDictionary<string, T>();

        public Task PutAsync(string key, T value, CancellationToken token = default)
        {
            store.AddOrUpdate(key, value, (_, _) => value);
            return Task.CompletedTask;
        }

        public Task<T> GetAsync(string key, CancellationToken token = default)
        {
            store.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public Task DeleteAsync(string key, CancellationToken token = default)
        {
            store.TryRemove(key, out var _);
            return Task.CompletedTask;
        }
    }
}
