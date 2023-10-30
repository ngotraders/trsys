using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure.Redis;

namespace Trsys.Infrastructure.WriteModel.Tokens.Redis
{
    public class RedisTokenConnectionManagerStore : ISecretKeyConnectionManagerStore
    {
        private readonly ConcurrentDictionary<Guid, DateTimeOffset> lastAccessed = new();
        private readonly IConnectionMultiplexer connection;
        private static readonly RedisKey lastAccessedKey = RedisHelper.GetKey("TokenConnectionManagerStore:LastAccessed");
        private static readonly TimeSpan fiveSeconds = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);

        public RedisTokenConnectionManagerStore(IConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public async Task<bool> UpdateLastAccessedAsync(Guid id)
        {
            var now = DateTimeOffset.UtcNow;
            if (!lastAccessed.TryGetValue(id, out var timestamp))
            {
                lastAccessed.TryAdd(id, now);
            }
            if (now - timestamp < oneSecond)
            {
                return false;
            }
            var cache = connection.GetDatabase();
            var idStr = id.ToString();
            return await cache.SortedSetAddAsync(lastAccessedKey, idStr, (now + fiveSeconds).ToUnixTimeSeconds(), When.Always);
        }

        public async Task<bool> ClearConnectionAsync(Guid id)
        {
            try
            {
                var cache = connection.GetDatabase();
                return await cache.SortedSetRemoveAsync(lastAccessedKey, id.ToString());
            }
            finally
            {
                // Remove cache most latest time
                lastAccessed.TryRemove(id, out var _);
            }
        }

        public async Task<List<Guid>> SearchExpiredSecretKeysAsync()
        {
            var cache = connection.GetDatabase();
            var values = await cache.SortedSetRangeByScoreAsync(lastAccessedKey, stop: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            return values.Select(v => Guid.TryParse(v.ToString(), out var result) ? result : Guid.Empty).ToList();
        }

        public async Task<List<Guid>> SearchConnectedSecretKeysAsync()
        {
            var cache = connection.GetDatabase();
            var setValues = await cache.SortedSetRangeByScoreAsync(lastAccessedKey);
            return setValues.Select(v => Guid.TryParse(v.ToString(), out var result) ? result : Guid.Empty).ToList();
        }

        public async Task<bool> IsConnectedAsync(Guid id)
        {
            var cache = connection.GetDatabase();
            var value = await cache.SortedSetScoreAsync(lastAccessedKey, id.ToString());
            return value.HasValue;
        }
    }
}