using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens
{
    interface ITokenConnectionManagerStore
    {
        Task<bool> TryAddAsync(string token, Guid id);
        Task<bool> TryRemoveAsync(string token);
        Task<(bool, Guid)> ExtendTokenExpirationTimeAsync(string token);
        Task<(bool, Guid)> ClearExpirationTimeAsync(string token);
        Task<List<string>> SearchExpiredTokensAsync();
    }
}
