using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Infrastructure;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens
{
    public class TokenConnectionManager : ITokenConnectionManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, TokenConnectionReporter> _reporters = new();
        private readonly IServiceScopeFactory serviceScopeFactory;
        private bool disposed = false;

        public TokenConnectionManager(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public void Touch(string token)
        {
            if (_reporters.TryGetValue(token, out var reporter))
            {
                reporter.Touch();
            }
        }

        public void Add(string token, Guid id)
        {
            _reporters.GetOrAdd(token, (_) =>
            {
                var reporter = new TokenConnectionReporter(id, token);
                reporter.Connected += OnConnected;
                reporter.Disconnected += OnDisconnected;
                return reporter;
            });
        }

        public void Remove(string token)
        {
            if (_reporters.TryRemove(token, out var reporter))
            {
                OnDisconnected(this, new TokenConnectionEventArgs(reporter.Id, token));
                reporter.Connected -= OnConnected;
                reporter.Disconnected -= OnDisconnected;
                reporter.Dispose();
            }
        }

        private async void OnDisconnected(object sender, TokenConnectionEventArgs e)
        {
            if (disposed)
            {
                return;
            }
            using var scope = serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new DisconnectSecretKeyCommand(e.Id, e.Token));
        }

        private async void OnConnected(object sender, TokenConnectionEventArgs e)
        {
            if (disposed)
            {
                return;
            }
            using var scope = serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ConnectSecretKeyCommand(e.Id, e.Token));
        }

        public void Dispose()
        {
            disposed = true;
            foreach (var key in _reporters.Keys)
            {
                Remove(key);
            }
            GC.SuppressFinalize(this);
        }
    }
}
