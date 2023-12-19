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

        public Task UpdateAsync(Guid id, string name, string username, string emailAddress, string role)
        {
            return queue.Enqueue(() =>
            {
                var item = ById[id];
                ByNormalizedUsername.Remove(item.Username.ToUpperInvariant());
                ByNormalizedEmailAddress.Remove(item.EmailAddress.ToUpperInvariant());
                item.Name = name;
                item.Username = username;
                item.EmailAddress = emailAddress;
                item.Role = role;
                ById[id] = item;
                ByNormalizedUsername.Add(item.Username.ToUpperInvariant(), item);
                ByNormalizedEmailAddress.Add(item.EmailAddress.ToUpperInvariant(), item);
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

        public Task<List<UserDto>> SearchAsync(int start, int end, string[] sort, string[] order)
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
                var query = List as IEnumerable<UserDto>;
                if (sort != null && order != null)
                {
                    for (var i = 0; i < sort.Length; i++)
                    {
                        var sortKey = sort[i];
                        var orderKey = order[i];
                        if (orderKey == "asc")
                        {
                            query = query.OrderBy(item => GetItemValue(item, sortKey));
                        }
                        else if (orderKey == "desc")
                        {
                            query = query.OrderByDescending(item => GetItemValue(item, sortKey));
                        }
                    }
                }
                return query.Skip(start).Take(end - start).ToList();
            });
        }

        private static object GetItemValue(UserDto item, string sortKey)
        {
            return sortKey switch
            {
                "id" => item.Id,
                "name" => item.Name,
                "username" => item.Username,
                "emailAddress" => item.EmailAddress,
                "role" => item.Role,
                _ => throw new InvalidOperationException($"Unknown sort key {sortKey}."),
            };
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
