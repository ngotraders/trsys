using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class InMemorySecretKeyDatabase : InMemoryDatabaseBase<SecretKeyDto, Guid>, ISecretKeyDatabase
    {
        private readonly Dictionary<string, SecretKeyDto> ByKey = new();
        private readonly Dictionary<string, SecretKeyDto> ByToken = new();
        private readonly ILogger<InMemorySecretKeyDatabase> logger;

        public InMemorySecretKeyDatabase(ILogger<InMemorySecretKeyDatabase> logger)
        {
            this.logger = logger;
        }
        public Task AddAsync(SecretKeyDto secretKey)
        {
            return AddAsync(secretKey.Id, secretKey, _ =>
            {
                ByKey.Add(secretKey.Key, secretKey);
            });
        }

        public Task UpdateKeyTypeAsync(Guid id, SecretKeyType keyType)
        {
            return UpdateAsync(id, item => item.KeyType = keyType);
        }

        public Task UpdateDescriptionAsync(Guid id, string description)
        {
            return UpdateAsync(id, item => item.Description = description);
        }

        public Task UpdateIsApprovedAsync(Guid id, bool isApproved)
        {
            return UpdateAsync(id, item => item.IsApproved = isApproved);
        }

        public Task UpdateIsConnectedAsync(Guid id, bool isConnected)
        {
            return UpdateAsync(id, item => item.IsConnected = isConnected);
        }

        public Task UpdateTokenAsync(Guid id, string token)
        {
            return UpdateAsync(id, item =>
            {
                if (string.IsNullOrEmpty(token))
                {
                    ByToken.Remove(item.Token);
                    item.Token = null;
                }
                else
                {
                    item.Token = token;
                    ByToken.Add(item.Token, item);
                }
            });
        }

        public Task RemoveAsync(Guid id)
        {
            return RemoveAsync(id, item =>
            {
                ByKey.Remove(item.Key);
            });
        }

        protected override object GetItemValue(SecretKeyDto item, string sortKey)
        {
            return sortKey switch
            {
                "id" => item.Id,
                "key" => item.Key,
                "keyType" => item.KeyType,
                "description" => item.Description,
                "isApproved" => item.IsApproved,
                "token" => item.Token,
                "isConnected" => item.IsConnected,
                _ => throw new ArgumentOutOfRangeException(nameof(sortKey)),
            };
        }

        public Task<SecretKeyDto> FindByKeyAsync(string key)
        {
            return Task.FromResult(ByKey.TryGetValue(key, out var value) ? value : null);
        }

        public Task<SecretKeyDto> FindByTokenAsync(string token)
        {

            return Task.FromResult(ByToken.TryGetValue(token, out var value) ? value : null);
        }
    }
}
