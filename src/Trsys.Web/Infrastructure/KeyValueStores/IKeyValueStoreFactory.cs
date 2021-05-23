namespace Trsys.Web.Infrastructure.KeyValueStores
{
    public interface IKeyValueStoreFactory
    {
        IKeyValueStore<T> Create<T>(string keyPrefix = null);
    }
}
