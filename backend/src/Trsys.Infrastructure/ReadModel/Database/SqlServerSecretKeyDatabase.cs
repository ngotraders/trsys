using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.Database
{
    public class SqlServerSecretKeyDatabase : ISecretKeyDatabase, IDisposable
    {

        private readonly ITrsysReadModelContext db;

        public SqlServerSecretKeyDatabase(ITrsysReadModelContext db)
        {
            this.db = db;
        }

        public Task AddAsync(SecretKeyDto user)
        {
            db.SecretKeys.Add(user);
            return db.SaveChangesAsync();
        }

        public async Task UpdateKeyTypeAsync(Guid id, SecretKeyType keyType)
        {
            var secretKey = await db.SecretKeys.FindAsync(id);
            if (secretKey == null)
            {
                throw new InvalidOperationException($"secret key {id} not found.");
            }
            secretKey.KeyType = keyType;
            await db.SaveChangesAsync();
        }

        public async Task UpdateDescriptionAsync(Guid id, string description)
        {
            var secretKey = await db.SecretKeys.FindAsync(id);
            if (secretKey == null)
            {
                throw new InvalidOperationException($"secret key {id} not found.");
            }
            secretKey.Description = description;
            await db.SaveChangesAsync();
        }

        public async Task UpdateIsApprovedAsync(Guid id, bool isApproved)
        {
            var secretKey = await db.SecretKeys.FindAsync(id);
            if (secretKey == null)
            {
                throw new InvalidOperationException($"secret key {id} not found.");
            }
            secretKey.IsApproved = isApproved;
            await db.SaveChangesAsync();
        }

        public async Task UpdateTokenAsync(Guid id, string token)
        {
            var secretKey = await db.SecretKeys.FindAsync(id);
            if (secretKey == null)
            {
                throw new InvalidOperationException($"secret key {id} not found.");
            }
            secretKey.Token = token;
            await db.SaveChangesAsync();
        }

        public async Task UpdateIsConnectedAsync(Guid id, bool isConnected)
        {
            var secretKey = await db.SecretKeys.FindAsync(id);
            if (secretKey == null)
            {
                throw new InvalidOperationException($"secret key {id} not found.");
            }
            secretKey.IsConnected = isConnected;
            await db.SaveChangesAsync();
        }

        public Task RemoveAsync(Guid id)
        {
            db.SecretKeys.Remove(new SecretKeyDto() { Id = id });
            return db.SaveChangesAsync();
        }

        public Task<int> CountAsync()
        {
            return db.SecretKeys.CountAsync();
        }

        public Task<List<SecretKeyDto>> SearchAsync()
        {
            return db.SecretKeys.ToListAsync();
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
            return db.SecretKeys.Skip(start).Take(end - start).ToListAsync();
        }

        public Task<SecretKeyDto> FindByIdAsync(Guid id)
        {
            return db.SecretKeys.Where(secretKey => secretKey.Id == id).FirstOrDefaultAsync();
        }

        public Task<SecretKeyDto> FindByKeyAsync(string key)
        {
            return db.SecretKeys.Where(secretKey => secretKey.Key == key).FirstOrDefaultAsync();
        }

        public Task<SecretKeyDto> FindByTokenAsync(string token)
        {
            return db.SecretKeys.Where(secretKey => secretKey.Token == token).FirstOrDefaultAsync();
        }
        public void Dispose()
        {
            db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

