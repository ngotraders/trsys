using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.WriteModel.Infrastructure;
using Trsys.Web.Models.WriteModel.Notifications;

namespace Trsys.Web.WriteModel.Handlers
{
    public class TokenConnectionEventHandler :
        INotificationHandler<SecretKeyTokenGenerated>,
        INotificationHandler<SecretKeyTokenInvalidated>,
        INotificationHandler<TokenTouched>
    {
        private readonly ITokenConnectionManager manager;

        public TokenConnectionEventHandler(ITokenConnectionManager manager)
        {
            this.manager = manager;
        }

        public Task Handle(SecretKeyTokenGenerated notification, CancellationToken cancellationToken)
        {
            manager.Add(notification.Token, notification.Id);
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyTokenInvalidated notification, CancellationToken cancellationToken)
        {
            manager.Remove(notification.Token);
            return Task.CompletedTask;
        }

        public Task Handle(TokenTouched notification, CancellationToken cancellationToken)
        {
            manager.Touch(notification.Token);
            return Task.CompletedTask;
        }
    }
}
