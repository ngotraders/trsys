using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Redis;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens.Redis
{
    public class RedisTokenConnectionManagerStore : ITokenConnectionManagerStore
    {
        private readonly IConnectionMultiplexer connection;
        private static readonly RedisKey tokenKey = RedisHelper.GetKey("TokenConnectionManagerStore:Tokens");
        private static readonly RedisKey lastAccessedKey = RedisHelper.GetKey("TokenConnectionManagerStore:LastAccessed");
        private static readonly TimeSpan fiveSeconds = TimeSpan.FromSeconds(5);

        public RedisTokenConnectionManagerStore(IConnectionMultiplexer connection)
        {
            this.connection = connection;
        }

        public Task<bool> TryAddAsync(string token, Guid id)
        {
            var cache = connection.GetDatabase();
            return cache.HashSetAsync(tokenKey, token, id.ToString(), When.NotExists);
        }

        public async Task<bool> TryRemoveAsync(string token)
        {
            var cache = connection.GetDatabase();
            if (await cache.HashDeleteAsync(tokenKey, token))
            {
                await cache.SortedSetRemoveAsync(lastAccessedKey, token);
                return true;
            }
            return false;
        }

        public async Task<(bool, Guid)> ExtendTokenExpirationTimeAsync(string token)
        {
            var cache = connection.GetDatabase();
            var value = await cache.HashGetAsync(tokenKey, token);
            if (value.HasValue)
            {
                if (await cache.SortedSetAddAsync(lastAccessedKey, token, (DateTimeOffset.UtcNow + fiveSeconds).ToUnixTimeSeconds(), When.Always))
                {
                    return (true, Guid.Parse(value.ToString()));
                }
                return (false, Guid.Parse(value.ToString()));
            }
            return (false, Guid.Empty);
        }

        public async Task<(bool, Guid)> ClearExpirationTimeAsync(string token)
        {
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
            return (false, Guid.Empty);
        }

        public async Task<List<string>> SearchExpiredTokensAsync()
        {
            var cache = connection.GetDatabase();
            var values = await cache.SortedSetRangeByScoreAsync(lastAccessedKey, stop: DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            return values.Select(v => v.ToString()).ToList();
        }
    }
}