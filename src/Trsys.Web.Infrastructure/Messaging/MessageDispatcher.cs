using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.Messaging;

namespace Trsys.Web.Infrastructure.Messaging
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly SemaphoreSlim queue = new(1);
        private readonly IMediator mediator;
        private readonly ILogger<MessageDispatcher> logger;
        private readonly HashSet<Guid> applyingMessages = new();
        private readonly HashSet<Guid> appliedMessages = new();

        public MessageDispatcher(IMediator mediator, ILogger<MessageDispatcher> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
        }

        public async Task DispatchAsync(PublishingMessage message, CancellationToken cancellationToken = default)
        {
            await queue.WaitAsync();
            try
            {
                if (appliedMessages.Contains(message.Id))
                {
                    logger.LogDebug("Application of message skipped. {@message}", message);
                    return;
                }
                if (!applyingMessages.Add(message.Id))
                {
                    logger.LogDebug("Application of message skipped. {@message}", message);
                    return;
                }
            }
            finally
            {
                queue.Release();
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

            await queue.WaitAsync();
            try
            {
                if (succeeded)
                {
                    appliedMessages.Add(message.Id);
                }
                applyingMessages.Remove(message.Id);
            }
            finally
            {
                queue.Release();
            }
        }
    }
}
