using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Infrastructure.Tokens
{
    public class TokenConnectionManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, TokenConnectionReporter> _reporters = new();
        private readonly IServiceScopeFactory serviceScopeFactory;

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
                reporter.Connected -= OnConnected;
                reporter.Disconnected -= OnDisconnected;
                reporter.Dispose();
            }
        }

        private void OnDisconnected(object sender, TokenConnectionEventArgs e)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            mediator.Send(new DisconnectSecretKeyCommand(e.Id, e.Token));
        }

        private void OnConnected(object sender, TokenConnectionEventArgs e)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            mediator.Send(new ConnectSecretKeyCommand(e.Id, e.Token));
        }

        public void Dispose()
        {
            foreach (var key in _reporters.Keys)
            {
                Remove(key);
            }
            GC.SuppressFinalize(this);
        }
    }
}
