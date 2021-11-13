using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.ReadModel.Database;
using Trsys.Web.Infrastructure.Redis;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Infrastructure;

namespace Trsys.Web.Infrastructure.ReadModel.Caching
{
    public class RedisCachedOrderDatabase : IOrderDatabase, IDisposable
    {
        private readonly IConnectionMultiplexer connection;
        private readonly SqlServerOrderDatabase db;

        private readonly RedisKey ordersKey = RedisHelper.GetKey("OrderDatabase:Orders");
        private readonly RedisKey publishedOrdersKey = RedisHelper.GetKey("OrderDatabase:PublishedOrders");
        private readonly RedisKey entryKey = RedisHelper.GetKey("OrderDatabase:Entry");
        private readonly RedisKey entryKeyV2 = RedisHelper.GetKey("OrderDatabase:EntryV2");

        public RedisCachedOrderDatabase(IConnectionMultiplexer connection, ITrsysReadModelContext db)
        {
            this.connection = connection;
            this.db = new SqlServerOrderDatabase(db);
        }
        public async Task AddAsync(OrderDto order)
        {
            await db.AddAsync(order);
            await UpdateCacheAsync();
        }

        public async Task<OrdersTextEntry> FindEntryAsync(string version)
        {
            var cache = connection.GetDatabase();
            var value = await cache.StringGetAsync(version == "v1" ? entryKey : entryKeyV2);
            if (value.HasValue)
            {
                return JsonConvert.DeserializeObject<OrdersTextEntry>(value.ToString());
            }
            else
            {
                var orderEntry = await db.FindEntryAsync(version);
                await cache.StringSetAsync(entryKey, JsonConvert.SerializeObject(orderEntry));
                return orderEntry;
            }
        }

        public async Task RemoveAsync(string id)
        {
            await db.RemoveAsync(id);
            await UpdateCacheAsync();
        }

        public async Task RemoveBySecretKeyAsync(Guid id)
        {
            await db.RemoveBySecretKeyAsync(id);
            await UpdateCacheAsync();
        }

        public async Task<List<OrderDto>> SearchAsync()
        {
            var cache = connection.GetDatabase();
            var value = await cache.StringGetAsync(ordersKey);
            if (value.HasValue)
            {
                return JsonConvert.DeserializeObject<List<OrderDto>>(value.ToString());
            }
            else
            {
                var orders = await db.SearchAsync();
                await cache.StringSetAsync(ordersKey, JsonConvert.SerializeObject(orders));
                return orders;
            }
        }

        public async Task<List<PublishedOrder>> SearchPublishedOrderAsync()
        {
            var cache = connection.GetDatabase();
            var value = await cache.StringGetAsync(publishedOrdersKey);
            if (value.HasValue)
            {
                return JsonConvert.DeserializeObject<List<PublishedOrder>>(value.ToString());
            }
            else
            {
                var publishedOrders = await db.SearchPublishedOrderAsync();
                await cache.StringSetAsync(entryKey, JsonConvert.SerializeObject(publishedOrders));
                return publishedOrders;
            }
        }

        private async Task UpdateCacheAsync()
        {
            var cache = connection.GetDatabase();
            var orders = await db.SearchAsync();
            var publishedOrders = orders.Select(order => order.Order).ToList();
            var orderEntry = OrdersTextEntry.Create(publishedOrders);
            await cache.StringSetAsync(ordersKey, JsonConvert.SerializeObject(orders));
            await cache.StringSetAsync(publishedOrdersKey, JsonConvert.SerializeObject(publishedOrders));
            await cache.StringSetAsync(entryKey, JsonConvert.SerializeObject(orderEntry));
        }

        public void Dispose()
        {
            db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
