using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.KeyValueStores
{
    public interface IKeyValueStore<T>
    {
        Task PutAsync(string key, T value, CancellationToken token = default);

        Task<T> GetAsync(string key, CancellationToken token = default);

        Task DeleteAsync(string key, CancellationToken token = default);
    }
}
