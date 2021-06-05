using MediatR;
using SqlStreamStore.Infrastructure;
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
        private static readonly TaskQueue quque = new();
        private readonly SecretKeyInMemoryDatabase db;

        public SecretKeyQueryHandler(SecretKeyInMemoryDatabase db)
        {
            this.db = db;
        }
        public Task Handle(SecretKeyCreated notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.Add(new SecretKeyDto()
                {
                    Id = notification.Id,
                    Key = notification.Key
                });
            });
        }

        public Task Handle(SecretKeyKeyTypeChanged notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.ById[notification.Id].KeyType = notification.KeyType;
            });
        }

        public Task Handle(SecretKeyDescriptionChanged notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.ById[notification.Id].Description = notification.Description;
            });
        }

        public Task Handle(SecretKeyApproved notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.ById[notification.Id].IsApproved = true;
            });
        }

        public Task Handle(SecretKeyTokenGenerated notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                var item = db.ById[notification.Id];
                item.Token = notification.Token;
                db.ByToken.Add(item.Token, item);
            });
        }

        public Task Handle(SecretKeyTokenInvalidated notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                var item = db.ById[notification.Id];
                db.ByToken.Remove(item.Token);
                item.Token = null;
            });
        }

        public Task Handle(SecretKeyRevoked notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.ById[notification.Id].IsApproved = false;
            });
        }

        public Task Handle(SecretKeyEaConnected notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.ById[notification.Id].IsConnected = true;
            });
        }

        public Task Handle(SecretKeyEaDisconnected notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.ById[notification.Id].IsConnected = false;
            });
        }

        public Task Handle(SecretKeyDeleted notification, CancellationToken cancellationToken)
        {
            return quque.Enqueue(() =>
            {
                db.Remove(notification.Id);
            });
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
