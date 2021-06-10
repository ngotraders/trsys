using System;
using System.Threading.Tasks;

namespace Trsys.Web.Models.WriteModel.Infrastructure
{
    public interface ISecretKeyConnectionStore
    {
        Task<bool> IsTokenInUseAsync(Guid id);
        Task ConnectAsync(Guid id);
        Task DisconnectAsync(Guid id);
    }
}