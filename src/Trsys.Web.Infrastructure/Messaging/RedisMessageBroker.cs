using MediatR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Redis;
using Trsys.Web.Models.Messaging;

namespace Trsys.Web.Infrastructure.Messaging
{
    public class RedisMessageBroker : IMessagePublisher, IDisposable
    {
        private readonly CancellationTokenSource isDisposed = new();
        private readonly ConcurrentDictionary<string, bool> arrivedStreamIds = new();

        private readonly IConnectionMultiplexer connection;
        private readonly RedisKey streamsKey = RedisHelper.GetKey("Message:Streams");
        private EventHandler<EventArgs> StreamArrived;
        private RedisValue? lastReadStream;
        private readonly IMediator mediator;
        private readonly ILogger<RedisMessageBroker> logger;

        public Task Task { get; }

        public RedisMessageBroker(IConnectionMultiplexer connection, IMediator mediator, ILogger<RedisMessageBroker> logger)
        {
            this.connection = connection;
            this.mediator = mediator;
            this.logger = logger;
            this.Task = Task.Run(PollingMessage);
        }

        private async Task PollingMessage()
        {
            var cache = connection.GetDatabase();
            var result = await cache.StreamRangeAsync(streamsKey, messageOrder: Order.Descending, count: 1);
            if (result.Any())
            {
                lastReadStream = result.LastOrDefault().Id;
            }
            while (!isDisposed.IsCancellationRequested)
            {
                await ReadMessages();
                await Task.Delay(10);
            }
        }

        private async Task ReadMessages()
        {
            var cache = connection.GetDatabase();
            var result = await cache.StreamReadAsync(streamsKey, lastReadStream ?? "0-0", 100);
            while (result.Any())
            {
                foreach (var entry in result)
                {
                    var message = new PublishingMessage();
                    foreach (var e in entry.Values)
                    {
                        switch (e.Name)
                        {
                            case "Id":
                                message.Id = Guid.Parse(e.Value);
                                break;
                            case "Type":
                                message.Type = e.Value;
                                break;
                            case "Data":
                                message.Data = e.Value;
                                break;
                            default:
                                break;
                        }
                    }
                    var notification = MessageConverter.ConvertToNotification(message);
                    try
                    {
                        logger.LogDebug("Applying message {@message}", notification);
                        await mediator.Publish(notification);
                        logger.LogDebug("Applied message {@message}", notification);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error on applying message {@message}", notification);
                    }
                    lastReadStream = entry.Id;
                    arrivedStreamIds.TryAdd(entry.Id, true);
                }
                result = await cache.StreamReadAsync(streamsKey, lastReadStream ?? "0-0", 100);
            }
            StreamArrived?.Invoke(this, EventArgs.Empty);
        }

        public async Task Enqueue(PublishingMessageEnvelope notification, CancellationToken cancellationToken)
        {
            if (!notification.Payload.Any())
            {
                return;
            }
            var cache = connection.GetDatabase();
            var streamIds = new List<string>();
            foreach (var n in notification.Payload)
            {
                var streamId = await cache.StreamAddAsync(streamsKey, new[] {
                    new  NameValueEntry("Id", n.Id.ToString()),
                    new  NameValueEntry("Type", n.Type),
                    new  NameValueEntry("Data", n.Data),
                 });
                streamIds.Add(streamId);
            }
            var streamIdsList = streamIds.ToArray();
            logger.LogDebug("Waiting streamIds to apply: {streamId}", streamIdsList);
            await Task.Run(() => WaitFor(streamIds));
            logger.LogDebug("StreamIds applied: {streamId}", streamIdsList);
        }

        private async Task WaitFor(List<string> streamIds)
        {
            var tcs = new TaskCompletionSource<bool>(isDisposed);
            var semaphore = new SemaphoreSlim(1);
            EventHandler<EventArgs> OnStreamArrived = async (s, e) =>
            {
                await CheckStreamIds(streamIds, tcs, semaphore);
            };
            try
            {
                StreamArrived += OnStreamArrived;
                await CheckStreamIds(streamIds, tcs, semaphore);
                await tcs.Task;
            }
            finally
            {
                StreamArrived -= OnStreamArrived;
            }
        }

        private async Task CheckStreamIds(List<string> streamIds, TaskCompletionSource<bool> tcs, SemaphoreSlim semaphore)
        {
            if (tcs.Task.IsCompleted)
            {
                return;
            }
            await semaphore.WaitAsync();
            try
            {
                foreach (var item in streamIds.ToArray())
                {
                    if (arrivedStreamIds.TryRemove(item, out var _))
                    {
                        streamIds.Remove(item);
                    }
                }
                if (!streamIds.Any())
                {
                    tcs.SetResult(true);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Dispose()
        {
            isDisposed.Cancel();
            GC.SuppressFinalize(this);
        }
    }
}
