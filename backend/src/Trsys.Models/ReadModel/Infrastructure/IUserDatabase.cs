using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface IUserDatabase
    {
        Task AddAsync(UserDto user);
        Task UpdateUserInfoAsync(Guid id, string name, string emailAddress);
        Task UpdatePasswordHashAsync(Guid id, string passwordHash);
        Task RemoveAsync(Guid id);
        Task<int> CountAsync();
        Task<List<UserDto>> SearchAsync();
        Task<List<UserDto>> SearchAsync(int start, int end);
        Task<UserDto> FindByIdAsync(Guid id);
        Task<UserDto> FindByNormalizedUsernameAsync(string username);
        Task<UserPasswordHashDto> GetUserPasswordHash(Guid id);
    }
}
