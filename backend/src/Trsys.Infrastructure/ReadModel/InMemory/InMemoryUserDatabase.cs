using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure.Queue;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class InMemoryUserDatabase : IUserDatabase, IDisposable
    {
        private readonly BlockingTaskQueue queue = new();
        private readonly List<UserDto> List = new();
        private readonly Dictionary<Guid, UserDto> ById = new();
        private readonly Dictionary<string, UserDto> ByNormalizedUsername = new();
        private readonly Dictionary<string, UserDto> ByNormalizedEmailAddress = new();
        private readonly Dictionary<Guid, UserPasswordHashDto> PasswordHashById = new();

        public Task AddAsync(UserDto userDto)
        {
            return queue.Enqueue(() =>
            {
                ById.Add(userDto.Id, userDto);
                ByNormalizedUsername.Add(userDto.Username.ToUpperInvariant(), userDto);
                ByNormalizedEmailAddress.Add(userDto.EmailAddress.ToUpperInvariant(), userDto);
                PasswordHashById.Add(userDto.Id, new UserPasswordHashDto()
                {
                    Id = userDto.Id,
                    PasswordHash = null,
                });
                List.Add(userDto);
            });
        }

        public Task UpdateUserInfoAsync(Guid id, string name, string emailAddress)
        {
            return queue.Enqueue(() =>
            {
                var user = ById[id];
                user.Name = name;
                user.EmailAddress = emailAddress;
            });
        }

        public Task UpdatePasswordHashAsync(Guid id, string passwordHash)
        {
            return queue.Enqueue(() =>
            {
                if (PasswordHashById.TryGetValue(id, out var value))
                {
                    value.PasswordHash = passwordHash;
                }
            });

        }

        public Task RemoveAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                var item = ById[id];
                ById.Remove(id);
                ByNormalizedUsername.Remove(item.Username.ToUpperInvariant());
                ByNormalizedEmailAddress.Remove(item.EmailAddress.ToUpperInvariant());
                List.RemoveAt(List.IndexOf(item));
            });
        }

        public Task<int> CountAsync()
        {
            return queue.Enqueue(() =>
            {
                return List.Count;
            });
        }

        public Task<List<UserDto>> SearchAsync()
        {
            return queue.Enqueue(() =>
            {
                return List;
            });
        }

        public Task<List<UserDto>> SearchAsync(int start, int end)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (end <= start)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }
            return queue.Enqueue(() =>
            {
                return List.Skip(start).Take(end - start).ToList();
            });
        }

        public Task<UserDto> FindByIdAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                return ById.TryGetValue(id, out var value) ? value : null;
            });
        }

        public Task<UserDto> FindByNormalizedUsernameAsync(string username)
        {
            return queue.Enqueue(() =>
            {
                return ByNormalizedUsername.TryGetValue(username.ToUpperInvariant(), out var value)
                    ? value
                    : ByNormalizedEmailAddress.TryGetValue(username.ToUpperInvariant(), out value)
                    ? value
                    : null;
            });
        }

        public Task<UserPasswordHashDto> GetUserPasswordHash(Guid id)
        {
            return queue.Enqueue(() =>
            {
                if (PasswordHashById.TryGetValue(id, out var value))
                {
                    return value;
                }
                return null;
            });
        }

        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
