using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Infrastructure;
using Trsys.Web.Models.WriteModel.Notifications;

namespace Trsys.Web.WriteModel.Handlers
{
    public class TokenConnectionEventHandler : INotificationHandler<TokenTouched>
    {
        private readonly ITokenConnectionManager manager;

        public TokenConnectionEventHandler(ITokenConnectionManager manager)
        {
            this.manager = manager;
        }

        public Task Handle(TokenTouched notification, CancellationToken cancellationToken)
        {
            manager.Touch(notification.Token);
            return Task.CompletedTask;
        }
    }
}
