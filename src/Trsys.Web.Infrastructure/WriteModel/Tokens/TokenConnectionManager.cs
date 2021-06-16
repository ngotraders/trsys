using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Infrastructure;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens
{
    public class TokenConnectionManager : ITokenConnectionManager, IDisposable
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly Timer timer;
        private bool isProcessing = false;

        public TokenConnectionManager(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            timer = new Timer(OnTick, null, 1000, 1000);
        }

        private async void OnTick(object state)
        {
            lock (this)
            {
                if (isProcessing)
                {
                    return;
                }
                isProcessing = true;
            }
            try
            {
                await DisconnectExpiredTokens();
            }
            finally
            {
                lock (this)
                {
                    isProcessing = false;
                }
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
