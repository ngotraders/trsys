using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Authentication
{
    public class InMemorySecretTokenStore : ISecretTokenStore
    {
        private class InternalTokenStore
        {
            private readonly Dictionary<string, SecretTokenInfo> store = new Dictionary<string, SecretTokenInfo>();

            public SecretTokenInfo Find(string token)
            {
                lock (store)
                {
                    if (store.TryGetValue(token, out var value))
                    {
                        value.SetIsInUse();
                        value.Access();
                    }
                    return value;
                }
            }

            public void Add(SecretTokenInfo info)
            {
                lock (store)
                {
                    store.Add(info.Token, info);
                }
            }

            public void Remove(string token)
            {
                lock (store)
                {
                    store.Remove(token);
                }
            }
        }

        private readonly InternalTokenStore store = new InternalTokenStore();

        public Task<string> RegisterTokenAsync(string secretKey, SecretKeyType keyType)
        {
            SecretTokenInfo info = new SecretTokenInfo()
            {
                SecretKey = secretKey,
                KeyType = keyType,
                Token = Guid.NewGuid().ToString()
            };
            lock (store)
            {
                store.Add(info);
            }
            return Task.FromResult(info.Token);
        }

        public Task<SecretTokenInfo> FindInfoAsync(string token)
        {
            var value = store.Find(token);
            return Task.FromResult(value);
        }

        public Task UnregisterAsync(string token)
        {
            store.Remove(token);
            return Task.CompletedTask;
        }
    }
}
