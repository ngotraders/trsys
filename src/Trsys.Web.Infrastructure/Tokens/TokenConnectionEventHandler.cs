using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Events;

namespace Trsys.Web.Infrastructure.Tokens
{
    public class TokenConnectionEventHandler :
        INotificationHandler<SecretKeyTokenGenerated>,
        INotificationHandler<SecretKeyTokenInvalidated>,
        INotificationHandler<TokenTouched>
    {
        private readonly TokenConnectionManager manager;

        public TokenConnectionEventHandler(TokenConnectionManager manager)
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
