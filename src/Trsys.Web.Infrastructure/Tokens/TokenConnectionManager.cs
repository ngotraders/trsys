using MediatR;
using System;
using System.Collections.Concurrent;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Infrastructure.Tokens
{
    public class TokenConnectionManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, TokenConnectionReporter> _reporters = new();
        private readonly IMediator mediator;

        public TokenConnectionManager(IMediator mediator)
        {
            this.mediator = mediator;
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
                var reporter = new TokenConnectionReporter(id);
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
            mediator.Send(new DisconnectSecretKeyCommand(e.Id));
        }

        private void OnConnected(object sender, TokenConnectionEventArgs e)
        {
            mediator.Send(new ConnectSecretKeyCommand(e.Id));
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
