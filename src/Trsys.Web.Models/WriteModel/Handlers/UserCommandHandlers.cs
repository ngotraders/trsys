using CQRSlite.Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Domain;
using Trsys.Web.Models.WriteModel.Extensions;

namespace Trsys.Web.Models.WriteModel.Handlers
{
    public class UserCommandHandlers :
        IRequestHandler<CreateUserIfNotExistsCommand, Guid>,
        IRequestHandler<CreateUserCommand, Guid>,
        IRequestHandler<ChangePasswordHashCommand>
    {
        private readonly IRepository repository;

        public UserCommandHandlers(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Guid> Handle(CreateUserIfNotExistsCommand request, CancellationToken cancellationToken = default)
        {
            var state = await repository.GetWorldState();
            if (state.GenerateSecretKeyIdIfNotExists(request.Username, out var userId))
            {
                var item = new UserAggregate(userId, request.Name, request.Username, request.Role);
                item.ChangePasswordHash(request.PasswordHash);
                await repository.Save(item, item.Version, cancellationToken);
                await repository.Save(state, null, cancellationToken);
                return userId;
            }
            return userId;
        }


        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
        {
            var state = await repository.GetWorldState();
            if (!state.GenerateSecretKeyIdIfNotExists(request.Username, out var userId))
            {
                throw new InvalidOperationException("user name already exists.");
            }
            var item = new UserAggregate(userId, request.Name, request.Username, request.Role);
            item.ChangePasswordHash(request.PasswordHash);
            await repository.Save(item, item.Version, cancellationToken);
            await repository.Save(state, null, cancellationToken);
            return userId;
        }

        public async Task<Unit> Handle(ChangePasswordHashCommand request, CancellationToken cancellationToken)
        {
            var item = await repository.Get<UserAggregate>(request.Id, cancellationToken);
            item.ChangePasswordHash(request.PasswordHash);
            await repository.Save(item, item.Version, cancellationToken);
            return Unit.Value;
        }
    }
}
