using SqlStreamStore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.SqlStreamStore;

namespace Trsys.Web.Infrastructure.InMemory
{
    public class InMemoryLatestStreamVersionHolder : ILatestStreamVersionHolder
    {
        private readonly TaskQueue queue = new();
        private readonly Dictionary<Guid, int> _store = new();

        public Task<int> GetLatestVersionAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                return _store.TryGetValue(id, out var version) ? version : -1;
            });
        }

        public Task PutLatestVersionAsync(Guid id, int version)
        {
            return queue.Enqueue(() =>
            {
                if (!_store.TryAdd(id, version))
                {
                    if (_store[id] < version)
                    {
                        _store[id] = version;
                    }
                }
            });
        }
    }
}
