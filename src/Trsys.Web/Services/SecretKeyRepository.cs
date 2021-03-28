using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Models;

namespace Trsys.Web.Services
{
    public class SecretKeyRepository : ISecretKeyRepository
    {
        private readonly TrsysContext db;

        public SecretKeyRepository(TrsysContext db)
        {
            this.db = db;
        }

        public IQueryable<SecretKey> All => db.SecretKeys;

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
    }
}
