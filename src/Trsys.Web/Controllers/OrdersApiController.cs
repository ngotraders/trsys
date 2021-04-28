using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trsys.Web.Caching;
using Trsys.Web.Filters;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [EaVersion("20210331")]
    public class OrdersApiController : ControllerBase
    {
        private readonly IOrderRepository repository;
        private readonly OrdersCacheManager cache;

        public OrdersApiController(IOrderRepository repository, OrdersCacheManager cache)
        {
            this.repository = repository;
            this.cache = cache;
        }

        [HttpGet]
        [Produces("text/plain")]
        [Authorize(AuthenticationSchemes = "SecretToken", Roles = "Subscriber")]
        public async Task<IActionResult> GetOrders()
        {
            if (cache.TryGetCache(out var cacheEntry))
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
                var orders = await repository.SearchAllAsync();
                cacheEntry = cache.UpdateOrdersCache(orders);
            }
            HttpContext.Response.Headers["ETag"] = $"\"{cacheEntry.Hash}\"";
            return Ok(cacheEntry.Text);
        }

        [HttpPost]
        [Consumes("text/plain")]
        [Authorize(AuthenticationSchemes = "SecretToken", Roles = "Publisher")]
        public async Task<IActionResult> PostOrder([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            var orders = new List<Order>();
            if (!string.IsNullOrEmpty(text))
            {
                foreach (var item in text.Split("@"))
                {
                    if (!Regex.IsMatch(item, @"^\d+:[A-Z]+:[01]:\d+(\.\d+)?:\d+(\.\d+)?:\d+"))
                    {
                        return BadRequest();
                    }
                    var splitted = item.Split(":");
                    var ticketNo = splitted[0];
                    var symbol = splitted[1];
                    var orderType = (OrderType)int.Parse(splitted[2]);
                    var price = splitted[3];
                    var lots = splitted[4];
                    var time = splitted[5];
                    orders.Add(new Order()
                    {
                        TicketNo = int.Parse(ticketNo),
                        Symbol = symbol,
                        OrderType = orderType,
                        Price = decimal.Parse(price).Normalize(),
                        Lots = decimal.Parse(lots).Normalize(),
                        Time = long.Parse(time),
                    });
                }
            }

            await repository.SaveOrdersAsync(orders);
            cache.UpdateOrdersCache(orders);
            return Ok();
        }
    }

    static class DecimalExtension
    {
        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }
    }
}
