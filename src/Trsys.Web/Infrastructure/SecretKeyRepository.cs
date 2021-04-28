using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure
{
    public class SecretKeyRepository : ISecretKeyRepository
    {
        private readonly TrsysContextProcessor processor;

        public SecretKeyRepository(TrsysContextProcessor processor)
        {
            this.processor = processor;
        }

        public Task<List<SecretKey>> SearchAllAsync()
        {
            return processor.Enqueue(db => db.SecretKeys.ToListAsync());
        }

        public Task<SecretKey> CreateNewSecretKeyAsync(SecretKeyType keyType)
        {
            var entity = new SecretKey()
            {
                KeyType = keyType,
                Key = Guid.NewGuid().ToString(),
                IsValid = false,
            };
            return Task.FromResult(entity);
        }

        public Task<SecretKey> FindBySecretKeyAsync(string secretKey)
        {
            return processor.Enqueue(db => db.SecretKeys.FirstOrDefaultAsync(e => e.Key == secretKey));
        }

        public Task SaveAsync(SecretKey entity)
        {
            return processor.Enqueue(db =>
            {
                if (entity.Id > 0)
                {
                    db.SecretKeys.Update(entity);
                }
                else
                {
                    db.SecretKeys.Add(entity);
                }
                return db.SaveChangesAsync();
            });
        }

        public Task RemoveAsync(SecretKey entity)
        {
            return processor.Enqueue(db =>
            {
                db.SecretKeys.Remove(entity);
                return db.SaveChangesAsync();
            });
        }
    }
}
