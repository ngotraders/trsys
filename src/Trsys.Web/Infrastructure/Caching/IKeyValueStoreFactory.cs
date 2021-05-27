namespace Trsys.Web.Infrastructure.Caching
{
    public interface IKeyValueStoreFactory
    {
        IKeyValueStore<T> Create<T>(string keyPrefix = null);
    }
}
