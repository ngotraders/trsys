﻿using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Infrastructure.ReadModel.Database;
using Trsys.Infrastructure.Redis;
using Trsys.Models;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.Caching
{
    public class RedisCachedSecretKeyDatabase : ISecretKeyDatabase, IDisposable
    {
        private readonly IConnectionMultiplexer connection;
        private readonly SqlServerSecretKeyDatabase db;
        private readonly RedisKey byIdKey = RedisHelper.GetKey("SecretKeyDatabase:ById");
        private readonly RedisKey byKeyKey = RedisHelper.GetKey("SecretKeyDatabase:ByKey");
        private readonly RedisKey byTokenKey = RedisHelper.GetKey("SecretKeyDatabase:ByToken");

        public RedisCachedSecretKeyDatabase(IConnectionMultiplexer connection, ITrsysReadModelContext db)
        {
            this.connection = connection;
            this.db = new SqlServerSecretKeyDatabase(db);
        }

        public async Task AddAsync(SecretKeyDto secretKey)
        {
            await db.AddAsync(secretKey);
            await UpdateCacheAsync(secretKey);
        }

        public async Task<SecretKeyDto> FindByIdAsync(Guid id)
        {
            var cache = connection.GetDatabase();
            var idValue = id.ToString();
            var value = await cache.HashGetAsync(byIdKey, idValue);
            if (value.HasValue)
            {
                return JsonConvert.DeserializeObject<SecretKeyDto>(value.ToString());
            }
            else
            {
                var secretKey = await db.FindByIdAsync(id);
                await UpdateCacheAsync(secretKey);
                return secretKey;
            }
        }

        public async Task<SecretKeyDto> FindByKeyAsync(string key)
        {
            var cache = connection.GetDatabase();
            var value = await cache.HashGetAsync(byKeyKey, key);
            if (value.HasValue)
            {
                return await FindByIdAsync(Guid.Parse(value.ToString()));
            }
            else
            {
                var secretKey = await db.FindByKeyAsync(key);
                await UpdateCacheAsync(secretKey);
                return secretKey;
            }
        }

        public async Task<SecretKeyDto> FindByTokenAsync(string token)
        {
            var cache = connection.GetDatabase();
            var value = await cache.HashGetAsync(byTokenKey, token);
            if (value.HasValue)
            {
                return await FindByIdAsync(Guid.Parse(value.ToString()));
            }
            else
            {
                var secretKey = await db.FindByTokenAsync(token);
                await UpdateCacheAsync(secretKey);
                return secretKey;
            }
        }

        public async Task RemoveAsync(Guid id)
        {
            await db.RemoveAsync(id);

            var cache = connection.GetDatabase();
            var idValue = id.ToString();
            var value = await cache.HashGetAsync(byIdKey, idValue);
            if (value.HasValue)
            {
                var secretKey = JsonConvert.DeserializeObject<SecretKeyDto>(value.ToString());
                if (!string.IsNullOrEmpty(secretKey.Token))
                {
                    await cache.HashDeleteAsync(byTokenKey, secretKey.Token);
                }
                await cache.HashDeleteAsync(byKeyKey, secretKey.Key);
                await cache.HashDeleteAsync(byIdKey, idValue);
            }
        }

        public Task<List<SecretKeyDto>> SearchAsync()
        {
            return db.SearchAsync();
        }

        public Task<PagedResultDto<SecretKeyDto>> SearchPagedAsync(int page, int perPage)
        {
            return db.SearchPagedAsync(page, perPage);
        }

        public async Task UpdateDescriptionAsync(Guid id, string description)
        {
            await db.UpdateDescriptionAsync(id, description);
            var secretKey = await db.FindByIdAsync(id);
            await UpdateCacheAsync(secretKey);
        }

        public async Task UpdateIsApprovedAsync(Guid id, bool isApproved)
        {
            await db.UpdateIsApprovedAsync(id, isApproved);
            var secretKey = await db.FindByIdAsync(id);
            await UpdateCacheAsync(secretKey);
        }

        public async Task UpdateIsConnectedAsync(Guid id, bool isConnected)
        {
            await db.UpdateIsConnectedAsync(id, isConnected);
            var secretKey = await db.FindByIdAsync(id);
            await UpdateCacheAsync(secretKey);
        }

        public async Task UpdateKeyTypeAsync(Guid id, SecretKeyType keyType)
        {
            await db.UpdateKeyTypeAsync(id, keyType);
            var secretKey = await db.FindByIdAsync(id);
            await UpdateCacheAsync(secretKey);
        }

        public async Task UpdateTokenAsync(Guid id, string token)
        {
            var secretKey = await FindByIdAsync(id);
            var oldToken = secretKey.Token;
            await db.UpdateTokenAsync(id, token);

            secretKey = await db.FindByIdAsync(id);
            if (!string.IsNullOrEmpty(oldToken))
            {
                var cache = connection.GetDatabase();
                await cache.HashDeleteAsync(byTokenKey, oldToken);
            }
            await UpdateCacheAsync(secretKey);
        }

        private async Task UpdateCacheAsync(SecretKeyDto secretKey)
        {
            var cache = connection.GetDatabase();
            var idValue = (RedisValue)secretKey.Id.ToString();
            await cache.HashSetAsync(byIdKey, idValue, JsonConvert.SerializeObject(secretKey));
            await cache.HashSetAsync(byKeyKey, secretKey.Key, idValue);
            if (!string.IsNullOrEmpty(secretKey.Token))
            {
                await cache.HashSetAsync(byTokenKey, secretKey.Token, idValue);
            }
        }

        public void Dispose()
        {
            db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
