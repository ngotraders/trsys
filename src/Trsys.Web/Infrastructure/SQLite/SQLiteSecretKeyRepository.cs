using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Infrastructure.Generic;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.SQLite
{
    public class SQLiteSecretKeyRepository : ISecretKeyRepository
    {
        private readonly TrsysContextProcessor processor;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public SQLiteSecretKeyRepository(TrsysContextProcessor processor, IServiceScopeFactory serviceScopeFactory)
        {
            this.processor = processor;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public Task<List<SecretKey>> SearchAllAsync()
        {
            return processor.Enqueue(db => GetRepository(db, repository => repository.SearchAllAsync()));
        }

        public Task<SecretKey> CreateNewSecretKeyAsync(SecretKeyType keyType)
        {
            return processor.Enqueue(db => GetRepository(db, repository => repository.CreateNewSecretKeyAsync(keyType)));
        }

        public Task<SecretKey> FindBySecretKeyAsync(string secretKey)
        {
            return processor.Enqueue(db => GetRepository(db, repository => repository.FindBySecretKeyAsync(secretKey)));
        }

        public Task SaveAsync(SecretKey entity)
        {
            return processor.Enqueue(db => GetRepository(db, repository => repository.SaveAsync(entity)));
        }

        public Task RemoveAsync(SecretKey entity)
        {
            return processor.Enqueue(db => GetRepository(db, repository => repository.RemoveAsync(entity)));
        }

        private Task<T> GetRepository<T>(TrsysContext db, Func<SecretKeyRepository, Task<T>> func)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var store = scope.ServiceProvider.GetRequiredService<ISecretKeyTokenStore>();
                return func(new SecretKeyRepository(db, store));
            }
        }

        private Task GetRepository(TrsysContext db, Func<SecretKeyRepository, Task> func)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var store = scope.ServiceProvider.GetRequiredService<ISecretKeyTokenStore>();
                return func(new SecretKeyRepository(db, store));
            }
        }
    }
}
