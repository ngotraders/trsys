using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.Redis
{
    public static class IDistributedCacheExtensions
    {
        public static async Task<T> GetObjectAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default)
        {
            var data = await cache.GetStringAsync(key, token);
            if (data == null)
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(data);
        }
        public static async Task SetObjectAsync<T>(this IDistributedCache cache, string key, T value, CancellationToken token = default)
        {
            await cache.SetStringAsync(key, JsonConvert.SerializeObject(value), token);
        }
    }
}
