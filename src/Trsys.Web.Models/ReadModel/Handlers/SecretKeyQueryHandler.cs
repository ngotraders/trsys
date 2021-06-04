using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<SecretKeyQueryHandler> logger;

        public SecretKeyQueryHandler(SecretKeyInMemoryDatabase db, ILogger<SecretKeyQueryHandler> logger)
        {
            this.db = db;
            this.logger = logger;
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
            if (db.ById.TryGetValue(notification.Id, out var item))
            {
                item.KeyType = notification.KeyType;
            }
            else
            {
                logger.LogError("SecretKeyKeyTypeChanged:secret key not found. {0}", notification.Id);
            }
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyDescriptionChanged notification, CancellationToken cancellationToken = default)
        {
            if (db.ById.TryGetValue(notification.Id, out var item))
            {
                item.Description = notification.Description;
            }
            else
            {
                logger.LogError("SecretKeyDescriptionChanged:secret key not found. {0}", notification.Id);
            }
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyApproved notification, CancellationToken cancellationToken = default)
        {
            if (db.ById.TryGetValue(notification.Id, out var item))
            {
                item.IsApproved = true;
            }
            else
            {
                logger.LogError("SecretKeyApproved:secret key not found. {0}", notification.Id);
            }
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyTokenGenerated notification, CancellationToken cancellationToken = default)
        {
            if (db.ById.TryGetValue(notification.Id, out var item))
            {
                item.Token = notification.Token;
                db.ByToken.Add(item.Token, item);
            }
            else
            {
                logger.LogError("SecretKeyTokenGenerated:secret key not found. {0}", notification.Id);
            }
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyTokenInvalidated notification, CancellationToken cancellationToken = default)
        {
            if (db.ById.TryGetValue(notification.Id, out var item))
            {
                db.ByToken.Remove(item.Token);
                item.Token = null;
            }
            else
            {
                logger.LogError("SecretKeyTokenInvalidated:secret key not found. {0}", notification.Id);
            }
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyRevoked notification, CancellationToken cancellationToken = default)
        {
            if (db.ById.TryGetValue(notification.Id, out var item))
            {
                item.IsApproved = false;
            }
            else
            {
                logger.LogError("SecretKeyRevoked:secret key not found. {0}", notification.Id);
            }
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyEaConnected notification, CancellationToken cancellationToken = default)
        {
            if (db.ById.TryGetValue(notification.Id, out var item))
            {
                item.IsConnected = true;
            }
            else
            {
                logger.LogError("SecretKeyEaConnected:secret key not found. {0}", notification.Id);
            }
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyEaDisconnected notification, CancellationToken cancellationToken = default)
        {
            if (db.ById.TryGetValue(notification.Id, out var item))
            {
                item.IsConnected = false;
            }
            else
            {
                logger.LogError("SecretKeyEaDisconnected:secret key not found. {0}", notification.Id);
            }
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
