using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Caching
{
    public class OrdersCacheManager
    {
        private readonly IMemoryCache cache;

        public OrdersCacheManager(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public bool TryGetCache(out OrdersCache cacheEntry)
        {
            return cache.TryGetValue(CacheKeys.ORDERS_CACHE, out cacheEntry);
        }

        public OrdersCache UpdateOrdersCache(List<Order> orders)
        {
            var responseText = string.Join("@", orders.Select(o => $"{o.TicketNo}:{o.Symbol}:{(int)o.OrderType}:{o.Price}:{o.Lots}:{o.Time}"));
            var ordersCache = new OrdersCache
            {
                Hash = CalculateHash(responseText),
                Text = responseText,
            };
            cache.Set(CacheKeys.ORDERS_CACHE, ordersCache);
            return ordersCache;
        }

        private string CalculateHash(string text)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
            var str = BitConverter.ToString(hash);
            str = str.Replace("-", string.Empty);
            return str;
        }
    }
}
