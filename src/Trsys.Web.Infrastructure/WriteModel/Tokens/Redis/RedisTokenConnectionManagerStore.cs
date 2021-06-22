using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Redis;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens.Redis
{
    public class RedisTokenConnectionManagerStore : ITokenConnectionManagerStore
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> lastAccessed = new();
        private readonly IConnectionMultiplexer connection;
        private static readonly RedisKey tokenKey = RedisHelper.GetKey("TokenConnectionManagerStore:Tokens");
        private static readonly RedisKey lastAccessedKey = RedisHelper.GetKey("TokenConnectionManagerStore:LastAccessed");
        private static readonly TimeSpan fiveSeconds = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);

        public RedisTokenConnectionManagerStore(IConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public Task<bool> TryAddAsync(string token, Guid id)
        {
            var cache = connection.GetDatabase();
            return cache.HashSetAsync(tokenKey, token, id.ToString(), When.NotExists);
        }

        public async Task<(bool, Guid)> TryRemoveAsync(string token)
        {
            var cache = connection.GetDatabase();
            var value = await cache.HashGetAsync(tokenKey, token);
            if (value.HasValue)
            {
                await cache.HashDeleteAsync(tokenKey, token);
                var id = Guid.Parse(value.ToString());
                if (await cache.SortedSetRemoveAsync(lastAccessedKey, token))
                {
                    return (true, id);
                }
                return (false, id);
            }
            return (false, Guid.Empty);
        }

        public async Task<(bool, Guid)> ExtendTokenExpirationTimeAsync(string token)
        {
            var now = DateTimeOffset.UtcNow;
            if (!lastAccessed.TryGetValue(token, out var timestamp))
            {
                lastAccessed.TryAdd(token, now);
            }
            if (now - timestamp < oneSecond)
            {
                return (false, Guid.Empty);
            }
            var cache = connection.GetDatabase();
            var value = await cache.HashGetAsync(tokenKey, token);
            if (value.HasValue)
            {
                if (await cache.SortedSetAddAsync(lastAccessedKey, token, (now + fiveSeconds).ToUnixTimeSeconds(), When.Always))
                {
                    return (true, Guid.Parse(value.ToString()));
                }
                return (false, Guid.Parse(value.ToString()));
            }
            return (false, Guid.Empty);
        }

        public async Task<(bool, Guid)> ClearExpirationTimeAsync(string token)
        {
            lastAccessed.TryRemove(token, out var _);
            var cache = connection.GetDatabase();
            var value = await cache.HashGetAsync(tokenKey, token);
            if (value.HasValue)
            {
                if (await cache.SortedSetRemoveAsync(lastAccessedKey, token))
                {
                    return (true, Guid.Parse(value.ToString()));
                }
                return (false, Guid.Parse(value.ToString()));
            }
            else
            {
                await cache.SortedSetRemoveAsync(lastAccessedKey, token);
                return (false, Guid.Empty);
            }
        }

        public async Task<List<string>> SearchExpiredTokensAsync()
        {
            var cache = connection.GetDatabase();
            var values = await cache.SortedSetRangeByScoreAsync(lastAccessedKey, stop: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            return values.Select(v => v.ToString()).ToList();
        }

        public async Task<List<(string, Guid)>> SearchConnectionsAsync()
        {
            var cache = connection.GetDatabase();
            var setValues = await cache.SortedSetRangeByScoreAsync(lastAccessedKey);
            var values = await cache.HashGetAllAsync(tokenKey);
            var availableValues = setValues.Select(v => v.ToString()).ToHashSet();
            return values
                .Where(value => availableValues.Contains(value.Name.ToString()))
                .Select(value => (value.Name.ToString(), Guid.Parse(value.Value.ToString())))
                .ToList();
        }

        public async Task<bool> IsTokenInUseAsync(string token)
        {
            var cache = connection.GetDatabase();
            var value = await cache.SortedSetScoreAsync(lastAccessedKey, token);
            return value.HasValue;
        }
    }
}