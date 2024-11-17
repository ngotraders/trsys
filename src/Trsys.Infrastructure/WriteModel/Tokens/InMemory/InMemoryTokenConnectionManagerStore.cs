using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure.Queue;

namespace Trsys.Infrastructure.WriteModel.Tokens.InMemory
{
    public class InMemoryTokenConnectionManagerStore : ISecretKeyConnectionManagerStore
    {
        private readonly BlockingTaskQueue queue = new();
        private readonly Dictionary<Guid, (EaConnection, DateTime)> store = [];
        private static readonly TimeSpan fiveSeconds = TimeSpan.FromSeconds(5);

        public Task<bool> UpdateLastAccessedAsync(Guid id, string eaState)
        {
            return queue.Enqueue(() =>
            {
                var value = DateTime.UtcNow + fiveSeconds;
                if (!store.TryGetValue(id, out var oldConnection))
                {
                    store[id] = (new EaConnection(id, eaState), value);
                    return true;
                }
                store[id] = (new EaConnection(id, eaState), value);
                return oldConnection.Item1.EaState != eaState;
            });
        }

        public Task<bool> ClearConnectionAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                return store.Remove(id);
            });
        }

        public Task<List<EaConnection>> SearchExpiredSecretKeysAsync()
        {
            return queue.Enqueue(() =>
            {
                return store
                    .Where(e => DateTime.UtcNow > e.Value.Item2)
                    .Select(e => e.Value.Item1)
                    .ToList();
            });
        }

        public Task<List<EaConnection>> SearchConnectedSecretKeysAsync()
        {
            return Task.FromResult(store.Values.Select(v => v.Item1).ToList());
        }

        public Task<bool> IsConnectedAsync(Guid id)
        {
            return Task.FromResult(store.TryGetValue(id, out var _));
        }
    }
}