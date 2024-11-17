using System;
using System.Threading.Tasks;

namespace Trsys.Models.WriteModel.Infrastructure
{
    public interface ISecretKeyConnectionManager
    {
        Task InitializeAsync();
        void Touch(Guid id, string eaState, bool forcePublishEvent);
        Task ReleaseAsync(Guid id);
        Task<bool> IsConnectedAsync(Guid id);
    }
}
