using System;
using System.Threading;

namespace Trsys.Web.Infrastructure.Tokens
{
    public class TokenConnectionReporter : IDisposable
    {
        public event EventHandler<TokenConnectionEventArgs> Connected;
        public event EventHandler<TokenConnectionEventArgs> Disconnected;
        private SynchronizationContext context = new SynchronizationContext();
        private readonly Guid id;
        private readonly Timer timer;
        private DateTime? lastAccessed;

        public TokenConnectionReporter(Guid id)
        {
            this.id = id;
            timer = new Timer(OnTick, null, 3000, 3000);
        }

        private void OnTick(object state)
        {
            lock (this)
            {
                if (lastAccessed.HasValue && DateTime.UtcNow - lastAccessed.Value > TimeSpan.FromSeconds(5))
                {
                    lastAccessed = null;
                    context.Post((_) => OnDisconnected(new TokenConnectionEventArgs(id)), null);
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
                lastAccessed = DateTime.UtcNow;
                context.Post((_) => OnConnected(new TokenConnectionEventArgs(id)), null);
            }
        }

        public void Dispose()
        {
            timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
