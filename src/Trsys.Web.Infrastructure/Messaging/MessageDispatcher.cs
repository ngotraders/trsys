using MediatR;
using Microsoft.Extensions.Logging;
using System;
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
                logger.LogDebug("Applying message {@message}", message);
                var notification = MessageConverter.ConvertToNotification(message);
                await mediator.Publish(notification, cancellationToken);
                logger.LogDebug("Applied message {@message}", message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on applying message {@message}", message);
            }
            finally
            {
                queue.Release();
            }
        }
    }
}
