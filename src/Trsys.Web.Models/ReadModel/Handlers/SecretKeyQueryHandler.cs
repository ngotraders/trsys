using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Events;
using Trsys.Web.Models.ReadModel.Infrastructure;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Models.ReadModel.Handlers
{
    public class SecretKeyQueryHandler :
        INotificationHandler<SecretKeyCreated>,
        INotificationHandler<SecretKeyKeyTypeChanged>,
        INotificationHandler<SecretKeyDescriptionChanged>,
        INotificationHandler<SecretKeyApproved>,
        INotificationHandler<SecretKeyTokenGenerated>,
        INotificationHandler<SecretKeyTokenInvalidated>,
        INotificationHandler<SecretKeyRevoked>,
        INotificationHandler<SecretKeyEaConnected>,
        INotificationHandler<SecretKeyEaDisconnected>,
        INotificationHandler<SecretKeyDeleted>,
        IRequestHandler<GetSecretKeys, List<SecretKeyDto>>,
        IRequestHandler<GetSecretKey, SecretKeyDto>,
        IRequestHandler<FindBySecretKey, SecretKeyDto>,
        IRequestHandler<FindByCurrentToken, SecretKeyDto>
    {
        private readonly SecretKeyInMemoryDatabase db;

        public SecretKeyQueryHandler(SecretKeyInMemoryDatabase db)
        {
            this.db = db;
        }
        public Task Handle(SecretKeyCreated notification, CancellationToken cancellationToken = default)
        {
            db.Add(new SecretKeyDto()
            {
                Id = notification.Id,
                Key = notification.Key
            });

            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyKeyTypeChanged notification, CancellationToken cancellationToken = default)
        {
            db.ById[notification.Id].KeyType = notification.KeyType;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyDescriptionChanged notification, CancellationToken cancellationToken = default)
        {
            db.ById[notification.Id].Description = notification.Description;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyApproved notification, CancellationToken cancellationToken = default)
        {
            db.ById[notification.Id].IsApproved = true;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyTokenGenerated notification, CancellationToken cancellationToken = default)
        {
            var item = db.ById[notification.Id];
            item.Token = notification.Token;
            db.ByToken.Add(item.Token, item);
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyTokenInvalidated notification, CancellationToken cancellationToken = default)
        {
            var item = db.ById[notification.Id];
            db.ByToken.Remove(item.Token);
            item.Token = null;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyRevoked notification, CancellationToken cancellationToken = default)
        {
            db.ById[notification.Id].IsApproved = false;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyEaConnected notification, CancellationToken cancellationToken = default)
        {
            db.ById[notification.Id].IsConnected = true;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyEaDisconnected notification, CancellationToken cancellationToken = default)
        {
            db.ById[notification.Id].IsConnected = false;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyDeleted notification, CancellationToken cancellationToken)
        {
            db.Remove(notification.Id);
            return Task.CompletedTask;
        }

        public Task<List<SecretKeyDto>> Handle(GetSecretKeys message, CancellationToken token = default)
        {
            return Task.FromResult(db.List);
        }

        public Task<SecretKeyDto> Handle(GetSecretKey request, CancellationToken cancellationToken)
        {
            return Task.FromResult(db.ById.TryGetValue(request.Id, out var value) ? value : null);
        }

        public Task<SecretKeyDto> Handle(FindBySecretKey request, CancellationToken cancellationToken)
        {
            return Task.FromResult(db.ByKey.TryGetValue(request.Key, out var value) ? value : null);
        }

        public Task<SecretKeyDto> Handle(FindByCurrentToken request, CancellationToken cancellationToken)
        {
            return Task.FromResult(db.ByToken.TryGetValue(request.Token, out var value) ? value : null);
        }
    }
}
