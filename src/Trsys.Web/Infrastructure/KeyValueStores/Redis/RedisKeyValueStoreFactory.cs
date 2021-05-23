using Microsoft.Extensions.Caching.Distributed;

namespace Trsys.Web.Infrastructure.KeyValueStores.Redis
{
    public class RedisKeyValueStoreFactory : IKeyValueStoreFactory
    {
        private readonly IDistributedCache cache;

        public RedisKeyValueStoreFactory(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public IKeyValueStore<T> Create<T>(string keyPrefix)
        {
            return new RedisKeyValueStore<T>(keyPrefix, cache);
        }
    }
}
