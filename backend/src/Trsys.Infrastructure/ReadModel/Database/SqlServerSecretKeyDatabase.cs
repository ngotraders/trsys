using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public Task<List<SecretKeyDto>> SearchAsync(int start, int end, string[] sort, string[] order)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (end <= start)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }
            var query = db.SecretKeys.AsQueryable();
            if (sort != null && order != null)
            {
                for (var i = 0; i < sort.Length; i++)
                {
                    var sortField = sort[i];
                    var orderField = order[i];
                    if (orderField == "asc")
                    {
                        query = query.OrderBy(GetItemField(sortField));
                    }
                    else if (orderField == "desc")
                    {
                        query = query.OrderByDescending(GetItemField(sortField));
                    }
                }
            }
            return query.Skip(start).Take(end - start).ToListAsync();
        }

        private static Expression<Func<SecretKeyDto, object>> GetItemField(string sortField)
        {
            return sortField switch
            {
                "id" => item => item.Id,
                "key" => item => item.Key,
                "keyType" => item => item.KeyType,
                "description" => item => item.Description,
                "isApproved" => item => item.IsApproved,
                "token" => item => item.Token,
                "isConnected" => item => item.IsConnected,
                _ => throw new InvalidOperationException($"sort field {sortField} not found."),
            };
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

