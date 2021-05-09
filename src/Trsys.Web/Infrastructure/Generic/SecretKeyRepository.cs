using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.Generic
{
    public class SecretKeyRepository : ISecretKeyRepository
    {
        private readonly TrsysContext db;

        public SecretKeyRepository(TrsysContext db)
        {
            this.db = db;
        }

        public Task<List<SecretKey>> SearchAllAsync()
        {
            return db.SecretKeys.ToListAsync();
        }

        public Task<SecretKey> CreateNewSecretKeyAsync(SecretKeyType keyType)
        {
            return Task.FromResult(SecretKey.Create(keyType));
        }

        public Task<SecretKey> FindBySecretKeyAsync(string secretKey)
        {
            return db.SecretKeys.FirstOrDefaultAsync(e => e.Key == secretKey);
        }

        public Task SaveAsync(SecretKey entity)
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
        }

        public Task RemoveAsync(SecretKey entity)
        {
            db.SecretKeys.Remove(entity);
            return db.SaveChangesAsync();
        }
    }
}
