using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.Caching.InMemory
{
    public class InMemoryKeyValueStore<T> : IKeyValueStore<T>
    {
        private readonly ConcurrentDictionary<string, T> store = new ConcurrentDictionary<string, T>();

        public Task PutAsync(string key, T value, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Task.CompletedTask;
            }
            store.AddOrUpdate(key, value, (_, _) => value);
            return Task.CompletedTask;
        }

        public Task<T> GetAsync(string key, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Task.FromResult(default(T));
            }
            store.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public Task DeleteAsync(string key, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Task.CompletedTask;
            }
            store.TryRemove(key, out var _);
            return Task.CompletedTask;
        }
    }
}
