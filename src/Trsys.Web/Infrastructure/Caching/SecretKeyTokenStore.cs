using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Infrastructure.Caching
{
    public class SecretKeyTokenStore : ISecretKeyTokenStore
    {
        private readonly IKeyValueStore<SecretKeyToken> store;

        public SecretKeyTokenStore(IKeyValueStoreFactory factory)
        {
            this.store = factory.Create<SecretKeyToken>("SecretKeyToken");
        }

        public Task SaveAsync(SecretKeyToken secretKeyToken)
        {
            return store.PutAsync(secretKeyToken.Key, secretKeyToken);
        }

        public Task<SecretKeyToken> FindAsync(string key)
        {
            return store.GetAsync(key);
        }

        public Task RemoveAsync(string key)
        {
            return store.DeleteAsync(key);
        }
    }
}
