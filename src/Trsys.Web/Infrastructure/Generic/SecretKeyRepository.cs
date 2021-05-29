using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.Generic
{
    public class SecretKeyRepository : ISecretKeyRepository
    {
        private readonly TrsysContext db;
        private readonly ISecretKeyTokenStore store;

        public SecretKeyRepository(TrsysContext db, ISecretKeyTokenStore store)
        {
            this.db = db;
            this.store = store;
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

        public async Task SaveAsync(SecretKey entity)
        {
            var secretKeyToken = await store.FindAsync(entity.Key);
            if (secretKeyToken == null)
            {
                secretKeyToken = new SecretKeyToken()
                {
                    KeyType = entity.KeyType,
                    Key = entity.Key,
                    IsValid = entity.IsValid,
                };
            }
            else
            {
                secretKeyToken.KeyType = entity.KeyType;
                secretKeyToken.IsValid = entity.IsValid;
            }
            await store.SaveAsync(secretKeyToken);

            if (entity.Id > 0)
            {
                db.SecretKeys.Update(entity);
            }
            else
            {
                db.SecretKeys.Add(entity);
            }
            await db.SaveChangesAsync();
        }

        public async Task RemoveAsync(SecretKey entity)
        {
            var secretKeyToken = await store.FindAsync(entity.Key);
            if (secretKeyToken != null)
            {
                await store.RemoveAsync(entity.Key);
            }

            db.SecretKeys.Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}
