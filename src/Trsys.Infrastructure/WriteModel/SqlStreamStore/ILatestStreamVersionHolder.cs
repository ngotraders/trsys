using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trsys.Infrastructure.WriteModel.SqlStreamStore
{
    public interface ILatestStreamVersionHolder
    {
        Task PutAsync(long currentPosition, Dictionary<Guid, int> latestVersions);
        Task PutLatestVersionAsync(Guid id, int version);
        Task<int> GetLatestVersionAsync(Guid id);
        Task<long> GetCurrentPositionAsync();
    }
}
