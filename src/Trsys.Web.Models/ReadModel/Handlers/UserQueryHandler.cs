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
    public class UserQueryHandler :
        INotificationHandler<UserCreated>,
        INotificationHandler<UserPasswordHashChanged>,
        IRequestHandler<GetUsers, List<UserDto>>,
        IRequestHandler<GetUser, UserDto>,
        IRequestHandler<FindByUsername, UserDto>
    {
        private static readonly TaskQueue quque = new();
        private readonly UserInMemoryDatabase db;

        public UserQueryHandler(UserInMemoryDatabase db)
        {
            this.db = db;
        }
        public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.Add(new UserDto()
                {
                    Id = notification.Id,
                    Name = notification.Name,
                    Username = notification.Username,
                    Role = notification.Role,
                });
            });
        }

        public Task Handle(UserPasswordHashChanged notification, CancellationToken cancellationToken = default)
        {
            return quque.Enqueue(() =>
            {
                db.ById[notification.Id].PasswordHash = notification.PasswordHash;
            });
        }

        public Task<List<UserDto>> Handle(GetUsers message, CancellationToken token = default)
        {
            return Task.FromResult(db.List);
        }

        public Task<UserDto> Handle(GetUser request, CancellationToken cancellationToken)
        {
            return Task.FromResult(db.ById.TryGetValue(request.Id, out var value) ? value : null);
        }

        public Task<UserDto> Handle(FindByUsername request, CancellationToken cancellationToken)
        {
            return Task.FromResult(db.ByUsername.TryGetValue(request.Username, out var value) ? value : null);
        }
    }
}
