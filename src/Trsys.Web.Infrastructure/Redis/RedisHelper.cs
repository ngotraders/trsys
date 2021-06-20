using StackExchange.Redis;

namespace Trsys.Web.Infrastructure.Redis
{
    internal static class RedisHelper
    {
        public static RedisKey GetKey(string key)
        {
            return new RedisKey($"Trsys.Web:{key}");
        }
    }
}