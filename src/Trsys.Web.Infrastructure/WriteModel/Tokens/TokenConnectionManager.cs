using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.Messaging;
using Trsys.Web.Models.WriteModel.Infrastructure;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens
{
    public class TokenConnectionManager : ISecretKeyConnectionManager, IDisposable
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
            var store = scope.ServiceProvider.GetRequiredService<ISecretKeyConnectionManagerStore>();
            var connectedKeys = await store.SearchConnectedSecretKeysAsync();
            if (connectedKeys.Any())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                foreach (var keyId in connectedKeys)
                {
                    await mediator.Publish(new SecretKeyEaConnected(keyId));
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
            var store = scope.ServiceProvider.GetRequiredService<ISecretKeyConnectionManagerStore>();
            var expiredKeys = await store.SearchExpiredSecretKeysAsync();
            foreach (var keyId in expiredKeys)
            {
                var clearExpirationResult = await store.ClearConnectionAsync(keyId);
                if (clearExpirationResult)
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Publish(PublishingMessageEnvelope.Create(new SecretKeyEaDisconnected(keyId)));
                }
            }
        }

        public async void Touch(Guid id, bool forcePublishEvent)
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ISecretKeyConnectionManagerStore>();
            var touchResult = await store.UpdateLastAccessedAsync(id);
            if (forcePublishEvent || touchResult)
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(PublishingMessageEnvelope.Create(new SecretKeyEaConnected(id)));
            }
        }

        public async Task RetainAsync(Guid id)
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ISecretKeyConnectionManagerStore>();
            await store.UpdateLastAccessedAsync(id);
        }

        public async Task ReleaseAsync(Guid id)
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ISecretKeyConnectionManagerStore>();
            var removeResult = await store.ClearConnectionAsync(id);
            if (removeResult)
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(PublishingMessageEnvelope.Create(new SecretKeyEaDisconnected(id)));
            }
        }

        public Task<bool> IsConnectedAsync(Guid id)
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ISecretKeyConnectionManagerStore>();
            return store.IsConnectedAsync(id);
        }

        public void Dispose()
        {
            timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
