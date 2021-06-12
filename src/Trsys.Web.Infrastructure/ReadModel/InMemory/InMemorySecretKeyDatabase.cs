using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Queue;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Infrastructure;

namespace Trsys.Web.Infrastructure.ReadModel.InMemory
{
    public class InMemorySecretKeyDatabase : ISecretKeyDatabase, IDisposable
    {
        private readonly BlockingTaskQueue queue = new();
        private readonly List<SecretKeyDto> List = new();
        private readonly Dictionary<Guid, SecretKeyDto> ById = new();
        private readonly Dictionary<string, SecretKeyDto> ByKey = new();
        private readonly Dictionary<string, SecretKeyDto> ByToken = new();

        public Task AddAsync(SecretKeyDto secretKey)
        {
            return queue.Enqueue(() =>
            {
                ById.Add(secretKey.Id, secretKey);
                ByKey.Add(secretKey.Key, secretKey);
                List.Add(secretKey);
            });
        }

        private Task UpdateAsync(Guid id, Action<SecretKeyDto> modification)
        {
            return queue.Enqueue(() =>
            {
                modification.Invoke(ById[id]);
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
            return queue.Enqueue(() =>
            {
                var item = ById[id];
                ById.Remove(id);
                ByKey.Remove(item.Key);
                List.RemoveAt(List.IndexOf(item));
            });
        }

        public Task<List<SecretKeyDto>> SearchAsync()
        {
            return Task.FromResult(List);
        }

        public Task<SecretKeyDto> FindByIdAsync(Guid id)
        {
            return Task.FromResult(ById.TryGetValue(id, out var value) ? value : null);
        }

        public Task<SecretKeyDto> FindByKeyAsync(string key)
        {
            return Task.FromResult(ByKey.TryGetValue(key, out var value) ? value : null);
        }

        public Task<SecretKeyDto> FindByTokenAsync(string token)
        {
            return Task.FromResult(ByToken.TryGetValue(token, out var value) ? value : null);
        }
        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
