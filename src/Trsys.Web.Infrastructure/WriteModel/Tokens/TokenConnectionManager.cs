using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Infrastructure;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens
{
    public class TokenConnectionManager : ITokenConnectionManager, IDisposable
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly Timer timer;
        private int isProcessing = 0;

        public TokenConnectionManager(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            timer = new Timer(OnTick, null, 1000, 1000);
        }

        public async Task InitializeAsync()
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ITokenConnectionManagerStore>();
            var connections = await store.SearchConnectionsAsync();
            if (connections.Any())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                foreach (var connection in connections)
                {
                    await mediator.Publish(new SecretKeyEaConnected(connection.Item2));
                }
            }
        }

        private async void OnTick(object state)
        {
            if (Interlocked.CompareExchange(ref isProcessing, 1, 0) == 1)
            {
                return;
            }
            try
            {
                await DisconnectExpiredTokens();
            }
            finally
            {
                Interlocked.Exchange(ref isProcessing, 0);
            }
        }

        private async Task DisconnectExpiredTokens()
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ITokenConnectionManagerStore>();
            var expiredTokens = await store.SearchExpiredTokensAsync();
            foreach (var token in expiredTokens)
            {
                var clearExpirationResult = await store.ClearExpirationTimeAsync(token);
                if (clearExpirationResult.Item1)
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(new DisconnectSecretKeyCommand(clearExpirationResult.Item2, token));
                }
            }
        }

        public async void Touch(string token)
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ITokenConnectionManagerStore>();
            var touchResult = await store.ExtendTokenExpirationTimeAsync(token);
            if (touchResult.Item1)
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new ConnectSecretKeyCommand(touchResult.Item2, token));
            }
        }

        public async void Add(string token, Guid id)
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ITokenConnectionManagerStore>();
            await store.TryAddAsync(token, id);
        }

        public async void Remove(string token)
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ITokenConnectionManagerStore>();
            await store.TryRemoveAsync(token);
        }

        public void Dispose()
        {
            timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
