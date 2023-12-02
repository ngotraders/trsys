using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure.Redis;

namespace Trsys.Infrastructure.WriteModel.SqlStreamStore.Redis
{
    public class RedisLatestStreamVersionHolder : ILatestStreamVersionHolder
    {
        private readonly IConnectionMultiplexer connection;

        public RedisLatestStreamVersionHolder(IConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public async Task<long> GetCurrentPositionAsync()
        {
            var cache = connection.GetDatabase();
            var value = await cache.StringGetAsync(RedisHelper.GetKey("LatestStreamVersionHolder:CurrentPosition"));
            return value.TryParse(out long val) ? val : -1;
        }

        public async Task<int> GetLatestVersionAsync(Guid id)
        {
            var cache = connection.GetDatabase();
            var value = await cache.StringGetAsync(RedisHelper.GetKey($"LatestStreamVersionHolder:LatestVersion:{id}"));
            return value.TryParse(out int val) ? val : -1;
        }

        public async Task PutAsync(long currentPosition, Dictionary<Guid, int> latestVersions)
        {
            var cache = connection.GetDatabase();
            RedisKey currentPositionKey = RedisHelper.GetKey("LatestStreamVersionHolder:CurrentPosition");
            await SetValueIfValueIsLessThanValue(cache, currentPositionKey, currentPosition);

            if (latestVersions.Any())
            {
                var key = RedisHelper.GetKey($"LatestStreamVersionHolder:LatestVersion");
                foreach (var entry in latestVersions)
                {
                    await SetHashValueIfHashValueIsLessThanValue(cache, key, entry.Key, entry.Value);
                }
            }
        }

        public async Task PutLatestVersionAsync(Guid id, int version)
        {
            var cache = connection.GetDatabase();
            var key = RedisHelper.GetKey($"LatestStreamVersionHolder:LatestVersion");
            await SetHashValueIfHashValueIsLessThanValue(cache, key, id, version);
        }

        private static async Task SetValueIfValueIsLessThanValue(IDatabase cache, RedisKey key, long version)
        {
            var value = await cache.StringGetAsync(key);
            if (!value.HasValue || !value.TryParse(out long redisLatestVersion) || redisLatestVersion < version)
            {
                await cache.StringSetAsync(key, version);
            }
        }

        private static async Task SetHashValueIfHashValueIsLessThanValue(IDatabase cache, RedisKey key, Guid id, int version)
        {
            var field = (RedisValue)id.ToString();
            var value = await cache.HashGetAsync(key, field);
            if (!value.HasValue || !value.TryParse(out long redisLatestVersion) || redisLatestVersion < version)
            {
                await cache.HashSetAsync(key, field, version);
            }
        }
    }
}
