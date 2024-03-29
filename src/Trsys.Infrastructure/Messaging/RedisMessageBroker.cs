using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Infrastructure.Redis;
using Trsys.Models.Messaging;

namespace Trsys.Infrastructure.Messaging
{
    public class RedisMessageBroker : IMessagePublisher, IDisposable
    {
        private readonly CancellationTokenSource isDisposed = new();
        private readonly ConcurrentDictionary<string, bool> arrivedStreamIds = new();

        private readonly IConnectionMultiplexer connection;
        private readonly ISubscriber subscriber;
        private readonly RedisKey streamsKey = RedisHelper.GetKey("Message:Streams");
        private EventHandler<EventArgs> StreamArrived;
        private readonly RedisChannel messageChannel = RedisChannel.Literal("Message:Subscription");
        private RedisValue? lastReadStream;
        private int isProcessing = 0;
        private readonly IMessageDispatcher dispatcher;
        private readonly ILogger<RedisMessageBroker> logger;

        public RedisMessageBroker(IConnectionMultiplexer connection, IMessageDispatcher dispatcher, ILogger<RedisMessageBroker> logger)
        {
            this.connection = connection;
            var cache = connection.GetDatabase();
            var result = cache.StreamRange(streamsKey, messageOrder: Order.Descending, count: 1);
            if (result.Any())
            {
                lastReadStream = result.LastOrDefault().Id;
            }
            this.dispatcher = dispatcher;
            this.logger = logger;
            this.subscriber = connection.GetSubscriber();
            subscriber.Subscribe(messageChannel, OnMessage);
        }

        private async void OnMessage(RedisChannel _, RedisValue message)
        {
            await Task.Run(async () =>
            {
                if (Interlocked.CompareExchange(ref isProcessing, 1, 0) == 1)
                {
                    logger.LogDebug("Ignored. Other process is processing message: {id}", message.ToString());
                    return;
                }
                try
                {
                    logger.LogDebug("Processing message: {id}", message.ToString());
                    await ReadMessages();
                }
                finally
                {
                    Interlocked.Exchange(ref isProcessing, 0);
                }
            });
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
                    await dispatcher.DispatchAsync(message);
                    lastReadStream = entry.Id;
                    arrivedStreamIds.TryAdd(entry.Id, true);
                }
                result = await cache.StreamReadAsync(streamsKey, lastReadStream ?? "0-0", 100);
            }
            StreamArrived?.Invoke(this, EventArgs.Empty);
        }

        public async Task Enqueue(PublishingMessageEnvelope notification, CancellationToken cancellationToken = default)
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
            var id = Guid.NewGuid().ToString();
            logger.LogDebug("Publishing message: {id}", id);
            await connection.GetSubscriber().PublishAsync(messageChannel, id);
            var streamIdsList = streamIds.ToArray();
            logger.LogDebug("Waiting streamIds to apply: {streamId}", streamIdsList);
            await Task.Run(() => WaitFor(streamIds), cancellationToken);
            logger.LogDebug("StreamIds applied: {streamId}", streamIdsList);
        }

        private async Task WaitFor(List<string> streamIds)
        {
            var tcs = new TaskCompletionSource<bool>(isDisposed);
            var semaphore = new SemaphoreSlim(1);
            async void OnStreamArrived(object s, EventArgs e)
            {
                await CheckStreamIds(streamIds, tcs, semaphore);
            }
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
                    tcs.TrySetResult(true);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Dispose()
        {
            subscriber?.Unsubscribe(messageChannel, OnMessage);
            isDisposed.Cancel();
            GC.SuppressFinalize(this);
        }
    }
}
