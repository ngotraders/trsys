using MediatR;
using Newtonsoft.Json;
using SqlStreamStore.Infrastructure;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public class RedisMessageBus : IMessageBus, IDisposable
    {
        private readonly TaskQueue queue = new();
        private readonly IMediator mediator;
        private readonly Task<ConnectionMultiplexer> connectionTask;

        public RedisMessageBus(IMediator mediator, RedisMessageOptions options)
        {
            this.mediator = mediator;
            connectionTask = Task.Run(async () =>
            {
                var conn = await ConnectionMultiplexer.ConnectAsync(options.Configuration);
                await conn.GetSubscriber().SubscribeAsync("Trsys.Web:events", OnMessage);
                return conn;
            });
        }

        private async void OnMessage(RedisChannel channel, RedisValue value)
        {
            var notification = MessageConverter.ConvertToEvent(JsonConvert.DeserializeObject<PublishedMessage>(value.ToString()));
            await mediator.Publish(notification);
        }

        public Task Publish(INotification notification, CancellationToken cancellationToken = default)
        {
            return Publish(MessageConverter.ConvertFromNotification(notification));
        }

        public Task Publish(PublishedMessage message, CancellationToken cancellation = default)
        {
            return queue.Enqueue(async () =>
            {
                var connection = await connectionTask;
                connection.GetSubscriber().Publish("Trsys.Web:events", JsonConvert.SerializeObject(new
                {
                    id = message.Id,
                    type = message.Type,
                    data = message.Data
                }));
            });
        }

        public void Dispose()
        {
            var conn = connectionTask.Result;
            conn.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
