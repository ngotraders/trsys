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
    public class UserQueryHandler :
        INotificationHandler<UserCreated>,
        INotificationHandler<UserUserInfoUpdated>,
        INotificationHandler<UserPasswordHashChanged>,
        IRequestHandler<GetUsers, SearchResponseDto<UserDto>>,
        IRequestHandler<GetUser, UserDto>,
        IRequestHandler<FindByUsername, UserDto>
    {
        private readonly IUserDatabase db;

        public UserQueryHandler(IUserDatabase db)
        {
            this.db = db;
        }
        public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
        {
            return db.AddAsync(new UserDto()
            {
                Id = notification.Id,
                Name = notification.Name,
                Username = notification.Username,
                Role = notification.Role,
            });
        }

        public Task Handle(UserPasswordHashChanged notification, CancellationToken cancellationToken = default)
        {
            return db.UpdatePasswordHashAsync(notification.Id, notification.PasswordHash);
        }

        public Task Handle(UserUserInfoUpdated notification, CancellationToken cancellationToken = default)
        {
            return db.UpdateUserInfoAsync(notification.Id, notification.Name, notification.EmailAddress);
        }

        public async Task<SearchResponseDto<UserDto>> Handle(GetUsers message, CancellationToken token = default)
        {
            var count = await db.CountAsync();
            if (message.Start.HasValue && message.End.HasValue)
            {
                return new SearchResponseDto<UserDto>(count, await db.SearchAsync(message.Start ?? 0, message.End ?? int.MaxValue));
            }
            else
            {
                return new SearchResponseDto<UserDto>(count, await db.SearchAsync());
            }
        }

        public Task<UserDto> Handle(GetUser request, CancellationToken cancellationToken)
        {
            return db.FindByIdAsync(request.Id);
        }

        public Task<UserDto> Handle(FindByUsername request, CancellationToken cancellationToken)
        {
            return db.FindByUsernameAsync(request.Username);
        }
    }
}
