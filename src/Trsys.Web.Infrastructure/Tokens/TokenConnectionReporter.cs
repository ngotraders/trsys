using System;
using System.Threading;

namespace Trsys.Web.Infrastructure.Tokens
{
    public class TokenConnectionReporter : IDisposable
    {
        public event EventHandler<TokenConnectionEventArgs> Connected;
        public event EventHandler<TokenConnectionEventArgs> Disconnected;
        private readonly Guid id;
        private readonly string token;
        private readonly Timer timer;
        private DateTime? lastAccessed;

        public TokenConnectionReporter(Guid id, string token)
        {
            this.id = id;
            this.token = token;
            timer = new Timer(OnTick, null, 1000, 1000);
        }

        private void OnTick(object state)
        {
            lock (this)
            {
                if (lastAccessed.HasValue && DateTime.UtcNow - lastAccessed.Value > TimeSpan.FromSeconds(5))
                {
                    lastAccessed = null;
                    OnDisconnected(new TokenConnectionEventArgs(id, token));
                }
            }
        }

        private void OnConnected(TokenConnectionEventArgs args)
        {
            Connected?.Invoke(this, args);
        }

        private void OnDisconnected(TokenConnectionEventArgs args)
        {
            Disconnected?.Invoke(this, args);
        }

        public void Touch()
        {
            lock (this)
            {
                if (!lastAccessed.HasValue)
                {
                    lastAccessed = DateTime.UtcNow;
                    OnConnected(new TokenConnectionEventArgs(id, token));
                }
                else
                {
                    lastAccessed = DateTime.UtcNow;
                }
            }
        }

        public void Dispose()
        {
            timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
