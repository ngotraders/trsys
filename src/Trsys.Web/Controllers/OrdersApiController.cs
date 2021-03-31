using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trsys.Web.Caching;
using Trsys.Web.Filters;
using Trsys.Web.Models;

namespace Trsys.Web.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [EaVersion("20210331")]
    public class OrdersApiController : ControllerBase
    {
        private readonly IOrderRepository repository;
        private readonly IMemoryCache cache;

        public OrdersApiController(IOrderRepository repository, IMemoryCache cache)
        {
            this.repository = repository;
            this.cache = cache;
        }

        [HttpGet]
        [Produces("text/plain")]
        [Authorize(AuthenticationSchemes = "SecretToken", Roles = "Subscriber")]
        public async Task<IActionResult> GetOrders()
        {
            if (cache.TryGetValue(CacheKeys.ORDERS_CACHE, out OrdersCache cacheEntry))
            {
                var etags = HttpContext.Request.Headers["If-None-Match"];
                if (etags.Any())
                {
                    foreach (var etag in etags)
                    {
                        if (etag == $"\"{cacheEntry.Hash}\"")
                        {
                            return StatusCode(304);
                        }
                    }
                }
            }
            else
            {
                var orders = await repository.All.ToListAsync();
                cacheEntry = RegisterCache(orders);
            }
            HttpContext.Response.Headers["ETag"] = $"\"{cacheEntry.Hash}\"";
            return Ok(cacheEntry.Text);
        }

        [HttpPost]
        [Consumes("text/plain")]
        [Authorize(AuthenticationSchemes = "SecretToken", Roles = "Publisher")]
        public async Task<IActionResult> PostOrder([FromBody] string text)
        {
            var orders = new List<Order>();
            var requestText = text.Trim(Convert.ToChar(0));
            if (!string.IsNullOrEmpty(requestText))
            {
                foreach (var item in requestText.Split("@"))
                {
                    if (!Regex.IsMatch(item, @"^\d+:[A-Z]+:[01]:\d+(\.\d+)?"))
                    {
                        return BadRequest();
                    }
                    var splitted = item.Split(":");
                    var ticketNo = splitted[0];
                    var symbol = splitted[1];
                    var orderType = (OrderType)int.Parse(splitted[2]);
                    var volumeCreditRate = splitted[3];
                    orders.Add(new Order()
                    {
                        TicketNo = int.Parse(ticketNo),
                        Symbol = symbol,
                        OrderType = orderType,
                        VolumeCreditRate = decimal.Parse(volumeCreditRate.TrimEnd('0')),
                    });
                }
            }

            await repository.SaveOrdersAsync(orders);
            RegisterCache(orders);
            return Ok();
        }

        private OrdersCache RegisterCache(List<Order> orders)
        {
            var responseText = string.Join("@", orders.Select(o => $"{o.TicketNo}:{o.Symbol}:{(int)o.OrderType}:{o.VolumeCreditRate}"));
            var ordersCache = new OrdersCache
            {
                Hash = CalculateHash(responseText),
                Text = responseText,
            };
            cache.Set(CacheKeys.ORDERS_CACHE, ordersCache);
            return ordersCache;
        }

        private string CalculateHash(string responseText)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(responseText));
            var str = BitConverter.ToString(hash);
            str = str.Replace("-", string.Empty);
            return str;
        }
    }
}
