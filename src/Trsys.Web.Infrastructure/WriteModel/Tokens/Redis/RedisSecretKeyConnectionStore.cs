using MediatR;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Redis;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.WriteModel.Infrastructure;

namespace Trsys.Web.Infrastructure.WriteModel.Tokens.Redis
{
    public class RedisSecretKeyConnectionStore : ISecretKeyConnectionStore
    {
        private readonly IConnectionMultiplexer connection;
        private readonly IMediator mediator;
        private readonly RedisKey storeKey = RedisHelper.GetKey("RedisSecretKeyConnectionStore");

        public RedisSecretKeyConnectionStore(IConnectionMultiplexer connection, IMediator mediator)
        {
            this.connection = connection;
            this.mediator = mediator;
        }

        public async Task ConnectAsync(Guid id)
        {
            var cache = connection.GetDatabase();
            var value = (RedisValue)id.ToString();
            if (await cache.SetAddAsync(storeKey, value))
            {
                await mediator.Publish(new SecretKeyEaConnected(id));
            }
        }

        public async Task DisconnectAsync(Guid id)
        {
            var cache = connection.GetDatabase();
            var value = (RedisValue)id.ToString();
            if (await cache.SetRemoveAsync(storeKey, value))
            {
                await mediator.Publish(new SecretKeyEaDisconnected(id));
            }
        }

        public async Task<bool> IsTokenInUseAsync(Guid id)
        {
            var cache = connection.GetDatabase();
            var value = (RedisValue)id.ToString();
            return await cache.SetContainsAsync(storeKey, value);
        }
    }
}