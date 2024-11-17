using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trsys.Infrastructure.WriteModel.Tokens
{
    public record EaConnection(Guid Id, string EaState);
    public interface ISecretKeyConnectionManagerStore
    {
        Task<bool> UpdateLastAccessedAsync(Guid id, string eaState);
        Task<bool> ClearConnectionAsync(Guid id);
        Task<List<EaConnection>> SearchExpiredSecretKeysAsync();
        Task<List<EaConnection>> SearchConnectedSecretKeysAsync();
        Task<bool> IsConnectedAsync(Guid id);
    }
}
