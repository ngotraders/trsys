using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Queue;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens.InMemory
{
    public class InMemoryTokenConnectionManagerStore : ISecretKeyConnectionManagerStore
    {
        private readonly BlockingTaskQueue queue = new();
        private readonly Dictionary<Guid, DateTime> store = new();
        private static readonly TimeSpan fiveSeconds = TimeSpan.FromSeconds(5);

        public Task<bool> UpdateLastAccessedAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                var value = DateTime.UtcNow + fiveSeconds;
                if (store.TryAdd(id, value))
                {
                    return true;
                }
                else
                {
                    store[id] = value;
                    return false;
                }
            });
        }

        public Task<bool> ClearConnectionAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                return store.Remove(id);
            });
        }

        public Task<List<Guid>> SearchExpiredSecretKeysAsync()
        {
            return queue.Enqueue(() =>
            {
                return store
                    .Where(e => DateTime.UtcNow > e.Value)
                    .Select(e => e.Key)
                    .ToList();
            });
        }

        public Task<List<Guid>> SearchConnectedSecretKeysAsync()
        {
            return Task.FromResult(store.Keys.ToList());
        }

        public Task<bool> IsConnectedAsync(Guid id)
        {
            return Task.FromResult(store.TryGetValue(id, out var _));
        }
    }
}