using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface ISecretKeyDatabase
    {
        Task AddAsync(SecretKeyDto secretKey);
        Task UpdateKeyTypeAsync(Guid id, SecretKeyType keyType);
        Task UpdateDescriptionAsync(Guid id, string description);
        Task UpdateIsApprovedAsync(Guid id, bool isApproved);
        Task UpdateTokenAsync(Guid id, string token);
        Task UpdateIsConnectedAsync(Guid id, bool isConnected);
        Task RemoveAsync(Guid id);
        Task<List<SecretKeyDto>> SearchAsync();
        Task<PagedResultDto<SecretKeyDto>> SearchPagedAsync(int page, int perPage);
        Task<SecretKeyDto> FindByIdAsync(Guid id);
        Task<SecretKeyDto> FindByKeyAsync(string key);
        Task<SecretKeyDto> FindByTokenAsync(string token);
    }
}
