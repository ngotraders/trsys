using CQRSlite.Domain;
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
        IRequestHandler<ConnectSecretKeyCommand>,
        IRequestHandler<DisconnectSecretKeyCommand>,
        IRequestHandler<DeleteSecretKeyCommand>
    {
        private readonly IRepository repository;
        private readonly ISecretKeyConnectionStore store;

        public SecretKeyCommandHandlers(IRepository repository, ISecretKeyConnectionStore store)
        {
            this.repository = repository;
            this.store = store;
        }

        public async Task<Guid> Handle(CreateSecretKeyIfNotExistsCommand request, CancellationToken cancellationToken = default)
        {
            var state = await repository.GetWorldState();
            var key = request.Key ?? Guid.NewGuid().ToString();
            if (state.GenerateSecretKeyIdIfNotExists(key, out var secretKeyId))
            {
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
                await repository.Save(state, null, cancellationToken);
                return secretKeyId;
            }
            return secretKeyId;
        }

        public async Task<Guid> Handle(CreateSecretKeyCommand request, CancellationToken cancellationToken = default)
        {
            var state = await repository.GetWorldState();
            var key = request.Key ?? Guid.NewGuid().ToString();
            if (!state.GenerateSecretKeyIdIfNotExists(key, out var secretKeyId))
            {
                throw new InvalidOperationException("specified key already exists.");
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
            await repository.Save(state, null, cancellationToken);
            return secretKeyId;
        }

        public async Task<Unit> Handle(UpdateSecretKeyCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            if (request.Approve.HasValue)
            {
                if (!request.Approve.Value)
                {
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
            return Unit.Value;
        }

        public async Task<string> Handle(GenerateSecretTokenCommand request, CancellationToken cancellationToken)
        {
            if (await store.IsTokenInUseAsync(request.Id))
            {
                throw new InvalidOperationException("Ea is already connected.");
            }
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            var token = item.GenerateToken();
            await repository.Save(item, item.Version, cancellationToken);
            return token;
        }

        public async Task<Unit> Handle(ConnectSecretKeyCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            if (item.Token == request.Token)
            {
                await store.ConnectAsync(request.Id);
            }
            return Unit.Value;
        }

        public async Task<Unit> Handle(DisconnectSecretKeyCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            if (string.IsNullOrEmpty(item.Token) || item.Token == request.Token)
            {
                await store.DisconnectAsync(request.Id);
            }
            return Unit.Value;
        }

        public async Task<Unit> Handle(InvalidateSecretTokenCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            item.InvalidateToken(request.Token);
            await repository.Save(item, item.Version, cancellationToken);
            if (!string.IsNullOrEmpty(request.Token))
            {
                await store.DisconnectAsync(request.Id);
            }
            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteSecretKeyCommand request, CancellationToken cancellationToken)
        {
            var state = await repository.GetWorldState();
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            state.DeleteSecretKey(item.Key, item.Id);
            item.Delete();
            await repository.Save(item, item.Version, cancellationToken);
            await repository.Save(state, null, cancellationToken);
            return Unit.Value;
        }
    }
}
