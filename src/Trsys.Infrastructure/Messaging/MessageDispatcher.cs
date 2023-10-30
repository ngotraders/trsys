using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.Messaging;

namespace Trsys.Infrastructure.Messaging
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IMediator mediator;
        private readonly ILogger<MessageDispatcher> logger;
        private readonly ConcurrentDictionary<Guid, bool> applyingMessages = new();
        private readonly ConcurrentDictionary<Guid, bool> appliedMessages = new();

        public MessageDispatcher(IMediator mediator, ILogger<MessageDispatcher> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
        }

        public async Task DispatchAsync(PublishingMessage message, CancellationToken cancellationToken = default)
        {
            if (appliedMessages.ContainsKey(message.Id))
            {
                logger.LogDebug("Application of message skipped. {@message}", message);
                return;
            }
            if (!applyingMessages.TryAdd(message.Id, true))
            {
                logger.LogDebug("Application of message skipped. {@message}", message);
                return;
            }

            bool succeeded = false;
            try
            {
                logger.LogDebug("Applying message. {@message}", message);
                var notification = MessageConverter.ConvertToNotification(message);
                await mediator.Publish(notification, cancellationToken);
                logger.LogDebug("Applied message. {@message}", message);
                succeeded = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error applying message. {@message}", message);
            }

            if (succeeded)
            {
                appliedMessages.TryAdd(message.Id, true);
            }
            applyingMessages.Remove(message.Id, out var _);
        }
    }
}
