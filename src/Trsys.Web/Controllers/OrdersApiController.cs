using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trsys.Web.Filters;
using Trsys.Web.Models.Orders;
using Trsys.Web.Services;

namespace Trsys.Web.Controllers
{
    [Route("api/orders")]
    [EaVersion("20210331")]
    [ApiController]
    public class OrdersApiController : ControllerBase
    {
        private readonly OrderService service;
        private readonly SecretKeyService secretKeyService;
        private readonly EventService eventService;

        public OrdersApiController(OrderService service, SecretKeyService secretKeyService, EventService eventService)
        {
            this.service = service;
            this.secretKeyService = secretKeyService;
            this.eventService = eventService;
        }

        [HttpGet]
        [Produces("text/plain")]
        [Authorize(AuthenticationSchemes = "SecretToken", Roles = "Subscriber")]
        public async Task<IActionResult> GetOrders()
        {
            await secretKeyService.TouchSecretTokenAsync(User.Identity.Name);
            var cacheEntry = await service.GetOrderTextEntryAsync();
            if (cacheEntry == null)
            {
                await eventService.RegisterSystemEventAsync("order", "CacheNotFound");
                await service.RefreshOrderTextAsync();
                throw new InvalidOperationException("Cache entry not found.");
            }
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

            await eventService.RegisterSystemEventAsync("order", "OrderFetched", new { SecretKey = User.Identity.Name, Text = cacheEntry.Text });
            HttpContext.Response.Headers["ETag"] = $"\"{cacheEntry.Hash}\"";
            return Ok(cacheEntry.Text);
        }

        [HttpPost]
        [Consumes("text/plain")]
        [Authorize(AuthenticationSchemes = "SecretToken", Roles = "Publisher")]
        public async Task<IActionResult> PostOrder([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            await secretKeyService.TouchSecretTokenAsync(User.Identity.Name);
            var orders = new List<Order>();
            if (!string.IsNullOrEmpty(text))
            {
                foreach (var item in text.Split("@"))
                {
                    if (!Regex.IsMatch(item, @"^\d+:[A-Z]+:[01]:\d+(\.\d+)?:\d+(\.\d+)?:\d+"))
                    {
                        await eventService.RegisterSystemEventAsync("order", "OrderUpdateFailed", new { SecretKey = User.Identity.Name, Text = text });
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

            await service.UpdateOrdersAsync(orders);
            await eventService.RegisterSystemEventAsync("order", "OrderUpdated", new { SecretKey = User.Identity.Name, Text = text });
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
