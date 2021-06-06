using SqlStreamStore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Infrastructure
{
    public class UserInMemoryDatabase : IUserDatabase
    {
        private readonly TaskQueue queueu = new();
        public readonly List<UserDto> List = new();
        public readonly Dictionary<Guid, UserDto> ById = new();
        public readonly Dictionary<string, UserDto> ByUsername = new();

        public Task AddAsync(UserDto userDto)
        {
            return queueu.Enqueue(() =>
            {
                ById.Add(userDto.Id, userDto);
                ByUsername.Add(userDto.Username, userDto);
                List.Add(userDto);
            });
        }

        public Task UpdatePasswordHashAsync(Guid id, string passwordHash)
        {
            return queueu.Enqueue(() =>
            {
                ById[id].PasswordHash = passwordHash;
            });

        }

        public Task RemoveAsync(Guid id)
        {
            return queueu.Enqueue(() =>
            {
                var item = ById[id];
                ById.Remove(id);
                ByUsername.Remove(item.Username);
                List.RemoveAt(List.IndexOf(item));
            });
        }

        public Task<List<UserDto>> SearchAsync()
        {
            return Task.FromResult(List);
        }

        public Task<UserDto> FindByIdAsync(Guid id)
        {
            return Task.FromResult(ById.TryGetValue(id, out var value) ? value : null);
        }

        public Task<UserDto> FindByUsernameAsync(string username)
        {
            return Task.FromResult(ByUsername.TryGetValue(username, out var value) ? value : null);
        }
    }
}
