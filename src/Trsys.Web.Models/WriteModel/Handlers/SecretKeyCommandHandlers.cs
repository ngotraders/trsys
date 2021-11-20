using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Domain;
using Trsys.Web.Models.WriteModel.Extensions;
using Trsys.Web.Models.WriteModel.Infrastructure;

namespace Trsys.Web.Models.WriteModel.Handlers
{
    public class SecretKeyCommandHandlers :
        IRequestHandler<CreateSecretKeyCommand, Guid>,
        IRequestHandler<CreateSecretKeyIfNotExistsCommand, Guid>,
        IRequestHandler<UpdateSecretKeyCommand>,
        IRequestHandler<GenerateSecretTokenCommand, string>,
        IRequestHandler<InvalidateSecretTokenCommand>,
        IRequestHandler<DeleteSecretKeyCommand>
    {
        private readonly IRepository repository;
        private readonly ISecretKeyConnectionManager connectionManager;

        public SecretKeyCommandHandlers(IRepository repository, ISecretKeyConnectionManager connectionManager)
        {
            this.repository = repository;
            this.connectionManager = connectionManager;
        }

        public async Task<Guid> Handle(CreateSecretKeyIfNotExistsCommand request, CancellationToken cancellationToken = default)
        {
            var state = await repository.GetWorldState();
            var key = request.Key ?? Guid.NewGuid().ToString();
            if (state.GenerateSecretKeyIdIfNotExists(key, out var secretKeyId))
            {
                await repository.Save(state, null, cancellationToken);
            }
            else
            {
                try
                {
                    await repository.Get<SecretKeyAggregate>(secretKeyId, cancellationToken);
                    return secretKeyId;
                }
                catch (AggregateNotFoundException)
                {
                }
            }

            var item = new SecretKeyAggregate(secretKeyId, key);
            if (request.KeyType.HasValue)
            {
                item.ChangeKeyType(request.KeyType.Value);
            }
            item.ChangeDescription(request.Description);
            if (request.Approve.HasValue)
            {
                if (request.Approve.Value)
                {
                    item.Approve();
                }
            }
            await repository.Save(item, item.Version, cancellationToken);
            return secretKeyId;
        }

        public async Task<Guid> Handle(CreateSecretKeyCommand request, CancellationToken cancellationToken = default)
        {
            var state = await repository.GetWorldState();
            var key = request.Key ?? Guid.NewGuid().ToString();
            if (state.GenerateSecretKeyIdIfNotExists(key, out var secretKeyId))
            {
                await repository.Save(state, null, cancellationToken);
            }
            else
            {
                try
                {
                    await repository.Get<SecretKeyAggregate>(secretKeyId, cancellationToken);
                    throw new InvalidOperationException("specified key already exists.");
                }
                catch (AggregateNotFoundException)
                {
                }
            }
            var item = new SecretKeyAggregate(secretKeyId, key);
            if (request.KeyType.HasValue)
            {
                item.ChangeKeyType(request.KeyType.Value);
            }
            item.ChangeDescription(request.Description);
            if (request.Approve.HasValue)
            {
                if (request.Approve.Value)
                {
                    item.Approve();
                }
            }
            await repository.Save(item, item.Version, cancellationToken);
            return secretKeyId;
        }

        public async Task<Unit> Handle(UpdateSecretKeyCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            if (request.Approve.HasValue)
            {
                if (!request.Approve.Value)
                {
                    if (!string.IsNullOrEmpty(item.Token))
                    {
                        item.InvalidateToken(item.Token);
                    }
                    item.Revoke();
                }
            }
            if (request.KeyType.HasValue)
            {
                item.ChangeKeyType(request.KeyType.Value);
            }
            item.ChangeDescription(request.Description);
            if (request.Approve.HasValue)
            {
                if (request.Approve.Value)
                {
                    item.Approve();
                }
            }
            await repository.Save(item, item.Version, cancellationToken);
            await connectionManager.ReleaseAsync(item.Id);
            return Unit.Value;
        }

        public async Task<string> Handle(GenerateSecretTokenCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            if (!string.IsNullOrEmpty(item.Token) && await connectionManager.IsConnectedAsync(request.Id))
            {
                throw new InvalidOperationException("Ea is already connected.");
            }
            var token = item.GenerateToken();
            await repository.Save(item, item.Version, cancellationToken);
            return token;
        }

        public async Task<Unit> Handle(InvalidateSecretTokenCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            item.InvalidateToken(request.Token);
            await repository.Save(item, item.Version, cancellationToken);
            await connectionManager.ReleaseAsync(request.Id);
            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteSecretKeyCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            item.Delete();
            await repository.Save(item, item.Version, cancellationToken);

            var state = await repository.GetWorldState();
            state.DeleteSecretKey(item.Key, item.Id);
            await repository.Save(state, null, cancellationToken);
            return Unit.Value;
        }
    }
}
