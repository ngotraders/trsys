using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.Events;
using Trsys.Models.Messaging;
using Trsys.Models.WriteModel.Infrastructure;

namespace Trsys.Infrastructure.WriteModel.Tokens
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
            var keyConnections = await store.SearchConnectedSecretKeysAsync();
            if (keyConnections.Count != 0)
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                foreach (var connection in keyConnections)
                {
                    await mediator.Publish(new SecretKeyEaConnected(connection.Id, connection.EaState));
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
            var keyConnections = await store.SearchExpiredSecretKeysAsync();
            foreach (var connection in keyConnections)
            {
                var clearExpirationResult = await store.ClearConnectionAsync(connection.Id);
                if (clearExpirationResult)
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Publish(PublishingMessageEnvelope.Create(new SecretKeyEaDisconnected(connection.Id)));
                }
            }
        }

        public async void Touch(Guid id, string eaState, bool forcePublishEvent)
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<ISecretKeyConnectionManagerStore>();
            var touchResult = await store.UpdateLastAccessedAsync(id, eaState);
            if (forcePublishEvent || touchResult)
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(PublishingMessageEnvelope.Create(new SecretKeyEaConnected(id, eaState)));
            }
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
