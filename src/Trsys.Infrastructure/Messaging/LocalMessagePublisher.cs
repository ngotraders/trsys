﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.Messaging;

namespace Trsys.Infrastructure.Messaging
{
    public class LocalMessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly SemaphoreSlim queue = new(1);
        private readonly IMessageDispatcher mediator;

        public LocalMessagePublisher(IMessageDispatcher mediator)
        {
            this.mediator = mediator;
        }

        public async Task Enqueue(PublishingMessageEnvelope notification, CancellationToken cancellationToken)
        {
            await queue.WaitAsync(cancellationToken);
            try
            {
                foreach (var n in notification.Payload)
                {
                    await mediator.DispatchAsync(n, cancellationToken);
                }
            }
            finally
            {
                queue.Release();
            }
        }

        public void Dispose()
        {
            queue.Wait();
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
