using MediatR;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly ISubscriber subscriber;
        private readonly RedisKey streamsKey = RedisHelper.GetKey("Message:Streams");
        private EventHandler<EventArgs> StreamArrived;
        private RedisChannel messageChannel = (string)RedisHelper.GetKey("Message:Subscription");
        private RedisValue? lastReadStream;
        private bool isProcessing;
        private IMediator mediator;

        public RedisMessageBroker(IConnectionMultiplexer connection, IMediator mediator)
        {
            this.connection = connection;
            this.subscriber = connection.GetSubscriber();
            this.mediator = mediator;
            subscriber.Subscribe(messageChannel, OnMessage);
        }

        private async void OnMessage(RedisChannel _, RedisValue message)
        {
            lock (this)
            {
                if (isProcessing)
                {
                    return;
                }
                isProcessing = true;
            }
            try
            {
                await ReadMessages();
            }
            finally
            {
                lock (this)
                {
                    isProcessing = false;
                }
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
                    try
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
                        await mediator.Publish(MessageConverter.ConvertToNotification(message));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
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
            await connection.GetSubscriber().PublishAsync(messageChannel, "OnMessage");
            await Task.Run(() => WaitFor(streamIds));
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
            subscriber?.Unsubscribe(messageChannel, OnMessage);
            isDisposed.Cancel();
            GC.SuppressFinalize(this);
        }
    }
}
