using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Queue;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens.InMemory
{
    public class InMemoryTokenConnectionManagerStore : ITokenConnectionManagerStore
    {
        private class TokenState
        {
            public string Token { get; set; }
            public Guid Id { get; set; }
            public DateTime? ExpiredAt { get; set; }
        }

        private readonly BlockingTaskQueue queue = new();
        private readonly Dictionary<string, TokenState> store = new Dictionary<string, TokenState>();
        private static TimeSpan fiveSeconds = TimeSpan.FromSeconds(5);

        public Task<bool> TryAddAsync(string token, Guid id)
        {
            return queue.Enqueue(() =>
            {
                return store.TryAdd(token, new TokenState()
                {
                    Token = token,
                    Id = id,
                });
            });
        }

        public Task<(bool, Guid)> TryRemoveAsync(string token)
        {
            return queue.Enqueue(() =>
            {
                if (store.TryGetValue(token, out var state))
                {
                    store.Remove(token);
                    if (state.ExpiredAt.HasValue)
                    {
                        return (false, state.Id);
                    }
                    else
                    {
                        return (true, state.Id);
                    }
                }
                return (false, Guid.Empty);
            });
        }

        public Task<(bool, Guid)> ExtendTokenExpirationTimeAsync(string token)
        {
            return queue.Enqueue(() =>
            {
                if (store.TryGetValue(token, out var state))
                {
                    if (state.ExpiredAt.HasValue)
                    {
                        state.ExpiredAt = DateTime.UtcNow + fiveSeconds;
                        return (false, state.Id);
                    }
                    else
                    {
                        state.ExpiredAt = DateTime.UtcNow + fiveSeconds;
                        return (true, state.Id);
                    }
                }
                return (false, Guid.Empty);
            });
        }

        public Task<(bool, Guid)> ClearExpirationTimeAsync(string token)
        {
            return queue.Enqueue(() =>
            {
                if (store.TryGetValue(token, out var state))
                {
                    if (state.ExpiredAt.HasValue)
                    {
                        state.ExpiredAt = null;
                        return (true, state.Id);
                    }
                    return (false, state.Id);
                }
                return (false, Guid.Empty);
            });
        }

        public Task<List<string>> SearchExpiredTokensAsync()
        {
            return queue.Enqueue(() =>
            {
                return store.Values
                    .Where(e => DateTime.UtcNow > e.ExpiredAt)
                    .Select(e => e.Token)
                    .ToList();
            });
        }

        public Task<List<(string, Guid)>> SearchConnectionsAsync()
        {
            var connections = store.Values
                .Where(e => e.ExpiredAt.HasValue)
                .Select(e => (e.Token, e.Id))
                .ToList();
            return Task.FromResult(connections);
        }

        public Task<bool> IsTokenInUseAsync(string token)
        {
            return Task.FromResult(store.Values.Any(e => e.Token == token && e.ExpiredAt.HasValue));
        }
    }
}