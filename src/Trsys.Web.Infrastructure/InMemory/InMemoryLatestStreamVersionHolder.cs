using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Queue;
using Trsys.Web.Infrastructure.SqlStreamStore;

namespace Trsys.Web.Infrastructure.InMemory
{
    public class InMemoryLatestStreamVersionHolder : ILatestStreamVersionHolder, IDisposable
    {
        private readonly BlockingTaskQueue queue = new();
        private readonly Dictionary<Guid, int> _store = new();
        private long currentPosition;

        public Task<int> GetLatestVersionAsync(Guid id)
        {
            return queue.Enqueue(() => _store.TryGetValue(id, out var version) ? version : -1);
        }
        public Task<long> GetCurrentPositionAsync()
        {
            return queue.Enqueue(() => currentPosition);
        }

        public Task PutAsync(long currentPosition, Dictionary<Guid, int> latestVersions)
        {
            return queue.Enqueue(() =>
            {
                foreach (var latestVersion in latestVersions)
                {
                    if (_store.TryGetValue(latestVersion.Key, out var version))
                    {
                        if (latestVersion.Value < version)
                        {
                            _store[latestVersion.Key] = version;
                        }
                    }
                    else
                    {
                        _store.Add(latestVersion.Key, latestVersion.Value);
                    }
                }
                if (currentPosition > this.currentPosition)
                {
                    this.currentPosition = currentPosition;
                }
            });
        }

        public Task PutLatestVersionAsync(Guid id, int newVersion)
        {
            return queue.Enqueue(() =>
            {
                if (_store.TryGetValue(id, out var version))
                {
                    if (newVersion < version)
                    {
                        _store[id] = version;
                    }
                }
                else
                {
                    _store.Add(id, newVersion);
                }
            });
        }

        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
