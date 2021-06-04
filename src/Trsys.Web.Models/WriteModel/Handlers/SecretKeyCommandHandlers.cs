﻿using CQRSlite.Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Domain;
using Trsys.Web.Models.WriteModel.Extensions;

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
        private readonly ISession session;
        private readonly IRepository repository;

        public SecretKeyCommandHandlers(ISession session, IRepository repository)
        {
            this.session = session;
            this.repository = repository;
        }

        public async Task<Guid> Handle(CreateSecretKeyIfNotExistsCommand request, CancellationToken cancellationToken = default)
        {
            var state = await session.GetWorldState();
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
                await session.Add(state, cancellationToken);
                await session.Add(item, cancellationToken);
                await session.Commit(cancellationToken);
                return secretKeyId;
            }
            return secretKeyId;
        }

        public async Task<Guid> Handle(CreateSecretKeyCommand request, CancellationToken cancellationToken = default)
        {
            var state = await session.GetWorldState();
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
            await session.Add(state, cancellationToken);
            await session.Add(item, cancellationToken);
            await session.Commit(cancellationToken);
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
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            var token = item.GenerateToken();
            await repository.Save(item, item.Version, cancellationToken);
            return token;
        }

        public async Task<Unit> Handle(ConnectSecretKeyCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            item.Connect(request.Token);
            await repository.Save(item, item.Version, cancellationToken);
            return Unit.Value;
        }

        public async Task<Unit> Handle(DisconnectSecretKeyCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            item.Disconnect(request.Token);
            await repository.Save(item, item.Version, cancellationToken);
            return Unit.Value;
        }

        public async Task<Unit> Handle(InvalidateSecretTokenCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            item.InvalidateToken(request.Token);
            await repository.Save(item, item.Version, cancellationToken);
            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteSecretKeyCommand request, CancellationToken cancellationToken)
        {
            var state = await session.GetWorldState();
            var item = await session.Get<SecretKeyAggregate>(request.Id, null, cancellationToken);
            state.DeleteSecretKey(item.Key, item.Id);
            item.Delete();
            await session.Add(state, cancellationToken);
            await session.Add(item, cancellationToken);
            await session.Commit(cancellationToken);
            return Unit.Value;
        }
    }
}