using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trsys.Web.Filters;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Services;

namespace Trsys.Web.Controllers
{
    [Route("api/orders")]
    [EaVersion("20210331")]
    [ApiController]
    public class OrdersApiController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly EventService eventService;

        public OrdersApiController(IMediator mediator, EventService eventService)
        {
            this.mediator = mediator;
            this.eventService = eventService;
        }

        [HttpGet]
        [Produces("text/plain")]
        [RequireToken("Subscriber")]
        public async Task<IActionResult> GetOrders()
        {
            var cacheEntry = await mediator.Send(new GetOrderTextEntry());
            if (cacheEntry == null)
            {
                await eventService.RegisterSystemEventAsync("order", "CacheNotFound");
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

            await mediator.Send(new FetchOrderCommand(Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), cacheEntry.Tickets));
            await eventService.RegisterSystemEventAsync("order", "OrderFetched", new { SecretKey = User.Identity.Name, Text = cacheEntry.Text });
            HttpContext.Response.Headers["ETag"] = $"\"{cacheEntry.Hash}\"";
            return Ok(cacheEntry.Text);
        }

        [HttpPost]
        [Consumes("text/plain")]
        [RequireToken("Publisher")]
        public async Task<IActionResult> PostOrder([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            var orders = new List<PublishedOrder>();
            if (!string.IsNullOrEmpty(text))
            {
                foreach (var item in text.Split("@"))
                {
                    var publishedOrder = PublishedOrder.Parse(item);
                    if (publishedOrder == null)
                    {
                        await eventService.RegisterSystemEventAsync("order", "OrderUpdateFailed", new { SecretKey = User.Identity.Name, Text = text });
                        return BadRequest();
                    }
                    orders.Add(publishedOrder);
                }
            }

            await mediator.Send(new PublishOrderCommand(Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), orders));
            await eventService.RegisterSystemEventAsync("order", "OrderUpdated", new { SecretKey = User.Identity.Name, Text = text });
            return Ok();
        }
    }
}
