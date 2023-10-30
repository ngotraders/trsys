using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface IUserDatabase
    {
        Task AddAsync(UserDto user);
        Task UpdatePasswordHashAsync(Guid id, string passwordHash);
        Task<List<UserDto>> SearchAsync();
        Task<UserDto> FindByIdAsync(Guid id);
        Task<UserDto> FindByUsernameAsync(string username);
    }
}
