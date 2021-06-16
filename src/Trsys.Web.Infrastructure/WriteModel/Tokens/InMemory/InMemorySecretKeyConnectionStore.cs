using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Trsys.Web.Infrastructure.Queue;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.Messaging;
using Trsys.Web.Models.WriteModel.Infrastructure;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens.InMemory
{
    public class InMemorySecretKeyConnectionStore : ISecretKeyConnectionStore
    {
        private readonly BlockingTaskQueue queue = new();
        private readonly HashSet<Guid> ConnectedIdSet = new();
        private readonly IMediator mediator;

        public InMemorySecretKeyConnectionStore(IMediator mediator)
        {
            this.mediator = mediator;
        }
        public Task ConnectAsync(Guid id)
        {
            return queue.Enqueue(async () =>
            {
                if (ConnectedIdSet.Add(id))
                {
                    await mediator.Publish(PublishingMessageEnvelope.Create(new SecretKeyEaConnected(id)));
                }
            });
        }

        public Task DisconnectAsync(Guid id)
        {
            return queue.Enqueue(async () =>
            {
                if (ConnectedIdSet.Remove(id))
                {
                    await mediator.Publish(PublishingMessageEnvelope.Create(new SecretKeyEaDisconnected(id)));
                }
            });
        }

        public Task<bool> IsTokenInUseAsync(Guid id)
        {
            return queue.Enqueue(() =>
            {
                return ConnectedIdSet.Contains(id);
            });
        }
    }
}