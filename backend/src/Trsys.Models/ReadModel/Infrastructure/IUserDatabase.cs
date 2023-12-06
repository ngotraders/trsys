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
        Task<List<UserDto>> SearchAsync();
        Task<List<UserDto>> SearchAsync(int page, int perPage);
        Task<UserDto> FindByIdAsync(Guid id);
        Task<UserDto> FindByUsernameAsync(string username);
        Task<int> CountAsync();
    }
}
