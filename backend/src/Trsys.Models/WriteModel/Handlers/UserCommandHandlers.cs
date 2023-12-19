using CQRSlite.Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.WriteModel.Commands;
using Trsys.Models.WriteModel.Domain;
using Trsys.Models.WriteModel.Extensions;

namespace Trsys.Models.WriteModel.Handlers
{
    public class UserCommandHandlers :
        IRequestHandler<UserCreateIfNotExistsCommand, Guid>,
        IRequestHandler<UserCreateCommand, Guid>,
        IRequestHandler<UserUpdateCommand>,
        IRequestHandler<UserDeleteCommand>,
        IRequestHandler<UserUpdateUserInfoCommand>,
        IRequestHandler<UserChangePasswordHashCommand>
    {
        private readonly IRepository repository;

        public UserCommandHandlers(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Guid> Handle(UserCreateIfNotExistsCommand request, CancellationToken cancellationToken = default)
        {
            var state = await repository.GetWorldState();
            if (state.GenerateSecretKeyIdIfNotExists(request.Username, out var userId))
            {
                var item = new UserAggregate(userId, request.Name, request.Username, request.EmailAddress, request.Role);
                item.ChangePasswordHash(request.PasswordHash);
                await repository.Save(item, item.Version, cancellationToken);
                await repository.Save(state, null, cancellationToken);
                return userId;
            }
            return userId;
        }


        public async Task<Guid> Handle(UserCreateCommand request, CancellationToken cancellationToken = default)
        {
            var state = await repository.GetWorldState();
            if (!state.GenerateUserIdIfNotExists(request.Username, out var userId))
            {
                throw new InvalidOperationException("user name already exists.");
            }
            var item = new UserAggregate(userId, request.Name, request.Username, request.EmailAddress, request.Role);
            item.ChangePasswordHash(request.PasswordHash);
            await repository.Save(item, item.Version, cancellationToken);
            await repository.Save(state, null, cancellationToken);
            return userId;
        }

        public async Task Handle(UserUpdateCommand request, CancellationToken cancellationToken = default)
        {
            var item = await repository.Get<UserAggregate>(request.Id, cancellationToken);
            var state = await repository.GetWorldState();
            if (!state.UpdateUser(item.Username, request.Username, request.Id))
            {
                throw new InvalidOperationException("user name already exists.");
            }
            item.Update(request.Name, request.Username, request.EmailAddress, request.Role);
            item.ChangePasswordHash(request.PasswordHash);
            await repository.Save(item, item.Version, cancellationToken);
            await repository.Save(state, null, cancellationToken);
        }

        public async Task Handle(UserDeleteCommand request, CancellationToken cancellationToken = default)
        {
            var item = await repository.Get<UserAggregate>(request.Id, cancellationToken);
            var state = await repository.GetWorldState();
            state.DeleteUser(item.Username, item.Id);
            item.Delete();
            await repository.Save(item, item.Version, cancellationToken);
            await repository.Save(state, null, cancellationToken);
        }


        public async Task Handle(UserUpdateUserInfoCommand request, CancellationToken cancellationToken = default)
        {
            var item = await repository.Get<UserAggregate>(request.Id, cancellationToken);
            item.UpdateUserInfo(request.Name, request.EmailAddress);
            await repository.Save(item, item.Version, cancellationToken);
        }

        public async Task Handle(UserChangePasswordHashCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<UserAggregate>(request.Id, cancellationToken);
            item.ChangePasswordHash(request.PasswordHash);
            await repository.Save(item, item.Version, cancellationToken);
        }
    }
}
