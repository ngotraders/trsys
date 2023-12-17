using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure.Queue;
using Trsys.Models;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class InMemorySecretKeyDatabase : ISecretKeyDatabase, IDisposable
    {
        private readonly BlockingTaskQueue queue = new();
        private readonly List<SecretKeyDto> List = new();
        private readonly Dictionary<Guid, SecretKeyDto> ById = new();
        private readonly Dictionary<string, SecretKeyDto> ByKey = new();
        private readonly Dictionary<string, SecretKeyDto> ByToken = new();
        private readonly ILogger<InMemorySecretKeyDatabase> logger;

        public InMemorySecretKeyDatabase(ILogger<InMemorySecretKeyDatabase> logger)
        {
            this.logger = logger;
        }
        public Task AddAsync(SecretKeyDto secretKey)
        {
            return queue.Enqueue(() =>
            {
                ById.Add(secretKey.Id, secretKey);
                ByKey.Add(secretKey.Key, secretKey);
                List.Add(secretKey);
            });
        }

        private Task<bool> UpdateAsync(Guid id, Action<SecretKeyDto> modification)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var value))
                {
                    modification.Invoke(value);
                    return true;
                }
                return false;
            });
        }

        public Task UpdateKeyTypeAsync(Guid id, SecretKeyType keyType)
        {
            return UpdateAsync(id, item => item.KeyType = keyType).ContinueWith(task =>
            {
                if (!task.Result)
                {
                    logger.LogWarning("UpdateKeyTypeAsync fail: {id}", id);
                }
            });
        }

        public Task UpdateDescriptionAsync(Guid id, string description)
        {
            return UpdateAsync(id, item => item.Description = description).ContinueWith(task =>
            {
                if (!task.Result)
                {
                    logger.LogWarning("UpdateDescriptionAsync fail: {id}", id);
                }
            });
        }

        public Task UpdateIsApprovedAsync(Guid id, bool isApproved)
        {
            return UpdateAsync(id, item => item.IsApproved = isApproved).ContinueWith(task =>
            {
                if (!task.Result)
                {
                    logger.LogWarning("UpdateIsApprovedAsync fail: {id}", id);
                }
            });
        }

        public Task UpdateIsConnectedAsync(Guid id, bool isConnected)
        {
            return UpdateAsync(id, item => item.IsConnected = isConnected).ContinueWith(task =>
            {
                if (!task.Result)
                {
                    logger.LogWarning("UpdateIsConnectedAsync fail: {id}", id);
                }
            });
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
            }).ContinueWith(task =>
            {
                if (!task.Result)
                {
                    logger.LogWarning("UpdateTokenAsync fail: {id}", id);
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

        public Task<int> CountAsync()
        {
            return queue.Enqueue(() =>
            {
                return List.Count;
            });
        }

        public Task<List<SecretKeyDto>> SearchAsync()
        {
            return queue.Enqueue(() =>
            {
                return List;
            });
        }

        public Task<List<SecretKeyDto>> SearchAsync(int start, int end)
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
                return List.Skip(start).Take(end - start).ToList();
            });
        }

        public Task<SecretKeyDto> FindByIdAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                return ById.TryGetValue(id, out var value) ? value : null;
            });
        }

        public Task<SecretKeyDto> FindByKeyAsync(string key)
        {
            return queue.Enqueue(() =>
            {
                return ByKey.TryGetValue(key, out var value) ? value : null;
            });
        }

        public Task<SecretKeyDto> FindByTokenAsync(string token)
        {

            return queue.Enqueue(() =>
            {
                return ByToken.TryGetValue(token, out var value) ? value : null;
            });
        }
        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
