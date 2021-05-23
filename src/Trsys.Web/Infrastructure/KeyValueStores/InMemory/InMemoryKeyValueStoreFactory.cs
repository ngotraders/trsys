using System;
using System.Collections.Concurrent;

namespace Trsys.Web.Infrastructure.KeyValueStores.InMemory
{
    public class InMemoryKeyValueStoreFactory : IKeyValueStoreFactory
    {
        private readonly ConcurrentDictionary<Type, object> store = new ConcurrentDictionary<Type, object>();
        public IKeyValueStore<T> Create<T>(string keyPrefix)
        {
            var value = store.GetOrAdd(typeof(T), _ => new InMemoryKeyValueStore<T>());
            return (IKeyValueStore<T>)value;
        }
    }
}
