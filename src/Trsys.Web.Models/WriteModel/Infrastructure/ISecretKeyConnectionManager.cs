using System;
using System.Threading.Tasks;

namespace Trsys.Web.Models.WriteModel.Infrastructure
{
    public interface ISecretKeyConnectionManager
    {
        Task InitializeAsync();
        void Touch(Guid id, bool forcePublishEvent);
        Task RetainAsync(Guid id);
        Task ReleaseAsync(Guid id);
        Task<bool> IsConnectedAsync(Guid id);
    }
}
