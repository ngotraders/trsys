using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.SQLite
{
    public class SQLiteSecretKeyRepository : ISecretKeyRepository
    {
        private readonly TrsysContextProcessor processor;

        public SQLiteSecretKeyRepository(TrsysContextProcessor processor)
        {
            this.processor = processor;
        }

        public Task<List<SecretKey>> SearchAllAsync()
        {
            return processor.Enqueue(db => db.SecretKeys.ToListAsync());
        }

        public Task<SecretKey> CreateNewSecretKeyAsync(SecretKeyType keyType)
        {
            return Task.FromResult(SecretKey.Create(keyType));
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
