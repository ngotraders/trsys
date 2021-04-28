using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.Authentication
{
    public class InMemorySecretTokenStore : ISecretTokenStore
    {
        private class InternalTokenStore
        {
            private readonly ConcurrentDictionary<string, SecretTokenInfo> store = new ConcurrentDictionary<string, SecretTokenInfo>();

            public SecretTokenInfo FindUpdatingAccessTime(string token)
            {
                if (store.TryGetValue(token, out var value))
                {
                    value.Access();
                }
                return value;
            }
            public SecretTokenInfo Find(string token)
            {
                store.TryGetValue(token, out var value);
                return value;
            }

            public void Add(SecretTokenInfo info)
            {
                store.TryAdd(info.Token, info);
            }

            public void Remove(string token)
            {
                store.Remove(token, out var _);
            }
        }

        private readonly InternalTokenStore store = new InternalTokenStore();

        public Task<string> RegisterTokenAsync(string secretKey, SecretKeyType keyType)
        {
            SecretTokenInfo info = new SecretTokenInfo(secretKey, keyType, Guid.NewGuid().ToString());
            info.Access();
            store.Add(info);
            return Task.FromResult(info.Token);
        }

        public Task<SecretTokenInfo> FindInfoAsync(string token)
        {
            var value = store.Find(token);
            return Task.FromResult(value);
        }

        public Task<SecretTokenInfo> FindInfoUpdatingAccessTimeAsync(string token)
        {
            var value = store.FindUpdatingAccessTime(token);
            return Task.FromResult(value);
        }

        public Task UnregisterAsync(string token)
        {
            store.Remove(token);
            return Task.CompletedTask;
        }
    }
}
