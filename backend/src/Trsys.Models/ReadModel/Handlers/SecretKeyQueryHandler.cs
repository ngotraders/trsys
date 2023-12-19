using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.Events;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;
using Trsys.Models.ReadModel.Queries;

namespace Trsys.Models.ReadModel.Handlers
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
        IRequestHandler<SearchSecretKeys, SearchResponseDto<SecretKeyDto>>,
        IRequestHandler<GetSecretKey, SecretKeyDto>,
        IRequestHandler<FindBySecretKey, SecretKeyDto>,
        IRequestHandler<FindByCurrentToken, SecretKeyDto>
    {
        private readonly ISecretKeyDatabase db;

        public SecretKeyQueryHandler(ISecretKeyDatabase db)
        {
            this.db = db;
        }

        public Task Handle(SecretKeyCreated notification, CancellationToken cancellationToken = default)
        {
            return db.AddAsync(new SecretKeyDto()
            {
                Id = notification.Id,
                Key = notification.Key
            });
        }

        public Task Handle(SecretKeyKeyTypeChanged notification, CancellationToken cancellationToken = default)
        {
            return db.UpdateKeyTypeAsync(notification.Id, notification.KeyType);
        }

        public Task Handle(SecretKeyDescriptionChanged notification, CancellationToken cancellationToken = default)
        {
            return db.UpdateDescriptionAsync(notification.Id, notification.Description);
        }

        public Task Handle(SecretKeyApproved notification, CancellationToken cancellationToken = default)
        {
            return db.UpdateIsApprovedAsync(notification.Id, true);
        }

        public Task Handle(SecretKeyTokenGenerated notification, CancellationToken cancellationToken = default)
        {
            return db.UpdateTokenAsync(notification.Id, notification.Token);
        }

        public Task Handle(SecretKeyTokenInvalidated notification, CancellationToken cancellationToken = default)
        {
            return db.UpdateTokenAsync(notification.Id, null);
        }

        public Task Handle(SecretKeyRevoked notification, CancellationToken cancellationToken = default)
        {
            return db.UpdateIsApprovedAsync(notification.Id, false);
        }

        public Task Handle(SecretKeyEaConnected notification, CancellationToken cancellationToken = default)
        {
            return db.UpdateIsConnectedAsync(notification.Id, true);
        }

        public Task Handle(SecretKeyEaDisconnected notification, CancellationToken cancellationToken = default)
        {
            return db.UpdateIsConnectedAsync(notification.Id, false);
        }

        public Task Handle(SecretKeyDeleted notification, CancellationToken cancellationToken = default)
        {
            return db.RemoveAsync(notification.Id);
        }

        public async Task<SearchResponseDto<SecretKeyDto>> Handle(SearchSecretKeys message, CancellationToken token = default)
        {
            var count = await db.CountAsync();
            if (message.Start.HasValue && message.End.HasValue)
            {
                return new SearchResponseDto<SecretKeyDto>(count, await db.SearchAsync(message.Start ?? 0, message.End ?? int.MaxValue, message.Sort, message.Order));
            }
            else
            {
                return new SearchResponseDto<SecretKeyDto>(count, await db.SearchAsync());
            }
        }

        public Task<SecretKeyDto> Handle(GetSecretKey request, CancellationToken cancellationToken)
        {
            return db.FindByIdAsync(request.Id);
        }

        public Task<SecretKeyDto> Handle(FindBySecretKey request, CancellationToken cancellationToken)
        {
            return db.FindByKeyAsync(request.Key);
        }

        public Task<SecretKeyDto> Handle(FindByCurrentToken request, CancellationToken cancellationToken)
        {
            return db.FindByTokenAsync(request.Token);
        }
    }
}
