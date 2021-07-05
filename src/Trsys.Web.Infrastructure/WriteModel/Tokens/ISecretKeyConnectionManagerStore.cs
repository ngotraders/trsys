using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens
{
    public interface ISecretKeyConnectionManagerStore
    {
        Task<bool> UpdateLastAccessedAsync(Guid id);
        Task<bool> ClearConnectionAsync(Guid id);
        Task<List<Guid>> SearchExpiredSecretKeysAsync();
        Task<List<Guid>> SearchConnectedSecretKeysAsync();
        Task<bool> IsConnectedAsync(Guid id);
    }
}
