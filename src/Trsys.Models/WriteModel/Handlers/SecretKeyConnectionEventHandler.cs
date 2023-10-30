using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.WriteModel.Infrastructure;
using Trsys.Models.WriteModel.Notifications;

namespace Trsys.Web.WriteModel.Handlers
{
    public class SecretKeyConnectionEventHandler : INotificationHandler<SecretKeyConnected>
    {
        private readonly ISecretKeyConnectionManager manager;

        public SecretKeyConnectionEventHandler(ISecretKeyConnectionManager manager)
        {
            this.manager = manager;
        }

        public Task Handle(SecretKeyConnected notification, CancellationToken cancellationToken)
        {
            manager.Touch(notification.Id, notification.ForcePublishEvent);
            return Task.CompletedTask;
        }
    }
}
