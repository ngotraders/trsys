using System;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public interface ILatestStreamVersionHolder
    {
        Task PutLatestVersionAsync(Guid id, int version);
        Task<int> GetLatestVersionAsync(Guid id);
    }
}
