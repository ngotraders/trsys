﻿using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, UserDto> ByUsername = new();

        public Task AddAsync(UserDto userDto)
        {
            return queue.Enqueue(() =>
            {
                ById.Add(userDto.Id, userDto);
                ByUsername.Add(userDto.Username.ToUpperInvariant(), userDto);
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
                ById[id].PasswordHash = passwordHash;
            });

        }

        public Task RemoveAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                var item = ById[id];
                ById.Remove(id);
                ByUsername.Remove(item.Username.ToUpperInvariant());
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
            return Task.FromResult(ByUsername.TryGetValue(username.ToUpperInvariant(), out var value) ? value : null);
        }
        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
