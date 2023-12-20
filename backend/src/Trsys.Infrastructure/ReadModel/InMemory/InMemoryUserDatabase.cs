using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class InMemoryUserDatabase : InMemoryDatabaseBase<UserDto, Guid>, IUserDatabase
    {
        private readonly Dictionary<string, UserDto> ByNormalizedUsername = [];
        private readonly Dictionary<string, UserDto> ByNormalizedEmailAddress = [];
        private readonly Dictionary<Guid, UserPasswordHashDto> PasswordHashById = [];

        public Task AddAsync(UserDto userDto)
        {
            return AddAsync(userDto.Id, userDto, _ =>
            {
                ByNormalizedUsername.Add(userDto.Username.ToUpperInvariant(), userDto);
                ByNormalizedEmailAddress.Add(userDto.EmailAddress.ToUpperInvariant(), userDto);
                PasswordHashById.Add(userDto.Id, new UserPasswordHashDto()
                {
                    Id = userDto.Id,
                    PasswordHash = null,
                });
            });
        }

        public Task UpdateAsync(Guid id, string name, string username, string emailAddress, string role)
        {
            return UpdateAsync(id, (item) =>
            {
                ByNormalizedUsername.Remove(item.Username.ToUpperInvariant());
                ByNormalizedEmailAddress.Remove(item.EmailAddress.ToUpperInvariant());
                item.Name = name;
                item.Username = username;
                item.EmailAddress = emailAddress;
                item.Role = role;
                ByNormalizedUsername.Add(item.Username.ToUpperInvariant(), item);
                ByNormalizedEmailAddress.Add(item.EmailAddress.ToUpperInvariant(), item);
            });
        }

        public Task UpdateUserInfoAsync(Guid id, string name, string emailAddress)
        {
            return UpdateAsync(id, user =>
            {
                user.Name = name;
                user.EmailAddress = emailAddress;
            });
        }

        public Task UpdatePasswordHashAsync(Guid id, string passwordHash)
        {
            return UpdateAsync(id, _ =>
            {
                if (PasswordHashById.TryGetValue(id, out var value))
                {
                    value.PasswordHash = passwordHash;
                }
            });

        }

        public Task RemoveAsync(Guid id)
        {
            return RemoveAsync(id, user =>
            {
                ByNormalizedUsername.Remove(user.Username.ToUpperInvariant());
                ByNormalizedEmailAddress.Remove(user.EmailAddress.ToUpperInvariant());
                PasswordHashById.Remove(user.Id);
            });
        }

        protected override object GetItemValue(UserDto item, string sortKey)
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
    }
}
