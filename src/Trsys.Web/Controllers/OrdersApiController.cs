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

namespace Trsys.Web.Controllers
{
    [Route("api/orders")]
    [EaEndpoint]
    [MinimumEaVersion("20210331")]
    [ApiController]
    public class OrdersApiController : ControllerBase
    {
        private readonly IMediator mediator;

        public OrdersApiController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [Produces("text/plain")]
        [RequireToken("Subscriber")]
        public async Task<IActionResult> GetOrders()
        {
            var cacheEntry = await mediator.Send(new GetOrderTextEntry());
            if (cacheEntry == null)
            {
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

            _ = Task.Run(() => mediator.Send(new FetchOrderCommand(Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), cacheEntry.Tickets)));
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
                        return BadRequest();
                    }
                    orders.Add(publishedOrder);
                }
            }

            await mediator.Send(new PublishOrderCommand(Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), orders));
            return Ok();
        }
    }
}
