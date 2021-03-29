using System;
using System.Collections.Generic;
using System.Linq;
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

        private class StoreProvider
        {
            private static char[] firstChars = new[]
            {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                'A',
                'B',
                'C',
                'D',
                'E',
                'F',
            };

            private readonly InternalTokenStore[] stores = Enumerable
                .Range(0, firstChars.Length)
                .Select(_ => new InternalTokenStore())
                .ToArray();
            private readonly InternalTokenStore defaultStore = new InternalTokenStore();

            public StoreProvider()
            {

            }

            private InternalTokenStore FindStoreForToken(string token)
            {
                var index = Array.IndexOf(firstChars, token.Substring(0, 1).ToUpper()[0]);
                return index >= 0 ? stores[index] : defaultStore;
            }

            public SecretTokenInfo Find(string token)
            {
                return FindStoreForToken(token).Find(token);
            }

            public void Add(SecretTokenInfo info)
            {
                FindStoreForToken(info.Token).Add(info);
            }

            public void Remove(string token)
            {
                FindStoreForToken(token).Remove(token);
            }
        }

        private readonly StoreProvider store = new StoreProvider();

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
