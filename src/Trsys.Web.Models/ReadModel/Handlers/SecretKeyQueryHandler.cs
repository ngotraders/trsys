using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Events;
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
        IRequestHandler<GetSecretKey, SecretKeyDto>
    {
        public Task Handle(SecretKeyCreated notification, CancellationToken cancellationToken = default)
        {
            SecretKeyInMemoryDatabase.Add(new SecretKeyDto()
            {
                Id = notification.Id,
                Key = notification.Key
            });

            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyKeyTypeChanged notification, CancellationToken cancellationToken = default)
        {
            SecretKeyInMemoryDatabase.Map[notification.Id].KeyType = notification.KeyType;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyDescriptionChanged notification, CancellationToken cancellationToken = default)
        {
            SecretKeyInMemoryDatabase.Map[notification.Id].Description = notification.Description;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyApproved notification, CancellationToken cancellationToken = default)
        {
            SecretKeyInMemoryDatabase.Map[notification.Id].ApprovedAt = notification.TimeStamp;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyTokenGenerated notification, CancellationToken cancellationToken = default)
        {
            SecretKeyInMemoryDatabase.Map[notification.Id].Token = notification.Token;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyTokenInvalidated notification, CancellationToken cancellationToken = default)
        {
            SecretKeyInMemoryDatabase.Map[notification.Id].Token = null;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyRevoked notification, CancellationToken cancellationToken = default)
        {
            SecretKeyInMemoryDatabase.Map[notification.Id].ApprovedAt = null;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyEaConnected notification, CancellationToken cancellationToken = default)
        {
            SecretKeyInMemoryDatabase.Map[notification.Id].IsConnected = true;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyEaDisconnected notification, CancellationToken cancellationToken = default)
        {
            SecretKeyInMemoryDatabase.Map[notification.Id].IsConnected = false;
            return Task.CompletedTask;
        }

        public Task Handle(SecretKeyDeleted notification, CancellationToken cancellationToken)
        {
            SecretKeyInMemoryDatabase.Remove(notification.Id);
            return Task.CompletedTask;
        }

        public Task<List<SecretKeyDto>> Handle(GetSecretKeys message, CancellationToken token = default)
        {
            return Task.FromResult(SecretKeyInMemoryDatabase.List);
        }

        public Task<SecretKeyDto> Handle(GetSecretKey request, CancellationToken cancellationToken)
        {
            return Task.FromResult(SecretKeyInMemoryDatabase.Map[request.Id]);
        }
    }
}
