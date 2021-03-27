using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Auth
{
    public class InMemorySecretTokenStore : ISecretTokenStore
    {
        private readonly Dictionary<string, SecretTokenInfo> store = new Dictionary<string, SecretTokenInfo>();

        public InMemorySecretTokenStore()
        {
        }

        public Task<string> RegisterTokenAsync(string secretKey, SecretKeyType keyType)
        {
            lock (store)
            {
                SecretTokenInfo info = new SecretTokenInfo()
                {
                    SecretKey = secretKey,
                    KeyType = keyType,
                    Token = Guid.NewGuid().ToString()
                };
                store.Add(info.Token, info);
                return Task.FromResult(info.Token);
            }
        }

        public Task<SecretTokenInfo> FindInfoAsync(string token)
        {
            lock (store)
            {
                if (store.TryGetValue(token, out var value))
                {
                    value.Access();
                    return Task.FromResult(value);
                }
                return Task.FromResult<SecretTokenInfo>(null);
            }
        }

        public Task UnregisterAsync(string token)
        {
            lock (store)
            {
                store.Remove(token);
                return Task.CompletedTask;
            }
        }
    }
}
