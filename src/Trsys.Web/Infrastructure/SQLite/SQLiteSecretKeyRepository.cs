using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Generic;
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
            return processor.Enqueue(db => new SecretKeyRepository(db).SearchAllAsync());
        }

        public Task<SecretKey> CreateNewSecretKeyAsync(SecretKeyType keyType)
        {
            return processor.Enqueue(db => new SecretKeyRepository(db).CreateNewSecretKeyAsync(keyType));
        }

        public Task<SecretKey> FindBySecretKeyAsync(string secretKey)
        {
            return processor.Enqueue(db => new SecretKeyRepository(db).FindBySecretKeyAsync(secretKey));
        }

        public Task SaveAsync(SecretKey entity)
        {
            return processor.Enqueue(db => new SecretKeyRepository(db).SaveAsync(entity));
        }

        public Task RemoveAsync(SecretKey entity)
        {
            return processor.Enqueue(db => new SecretKeyRepository(db).RemoveAsync(entity));
        }
    }
}
