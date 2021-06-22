using System;
using System.Threading.Tasks;

namespace Trsys.Web.Models.WriteModel.Infrastructure
{
    public interface ITokenConnectionManager
    {
        Task InitializeAsync();
        void Touch(string token);
        Task AddAsync(string token, Guid id);
        Task RemoveAsync(string token);
        Task<bool> IsTokenInUseAsync(string token);
    }
}
