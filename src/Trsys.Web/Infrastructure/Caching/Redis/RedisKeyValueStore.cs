using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.Caching.Redis
{
    public class RedisKeyValueStore<T> : IKeyValueStore<T>
    {
        private readonly IDistributedCache cache;
        private readonly Func<string, string> generateKey;

        public RedisKeyValueStore(string keyPrefix, IDistributedCache cache)
        {
            if (string.IsNullOrEmpty(keyPrefix))
            {
                this.generateKey = key => key;
            } else
            {
                this.generateKey = key => $"{keyPrefix}/{key}";
            }
            this.cache = cache;
        }

        public async Task PutAsync(string key, T value, CancellationToken token = default)
        {
            await cache.SetStringAsync(generateKey(key), JsonConvert.SerializeObject(value), token);
        }

        public async Task<T> GetAsync(string key, CancellationToken token = default)
        {
            var data = await cache.GetStringAsync(generateKey(key), token);
            if (data == null)
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(data);
        }

        public async Task DeleteAsync(string key, CancellationToken token = default)
        {
            await cache.RemoveAsync(generateKey(key), token);
        }
    }
}
