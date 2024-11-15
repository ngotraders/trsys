﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trsys.Web.Filters;
using Trsys.Models;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Web.Controllers
{
    [EaEndpoint]
    [MinimumEaVersion("20211109")]
    [ApiController]
    public class EaApiController : ControllerBase
    {
        private readonly ILogger<EaApiController> logger;
        private readonly IMediator mediator;

        public EaApiController(ILogger<EaApiController> logger, IMediator mediator)
        {
            this.logger = logger;
            this.mediator = mediator;
        }

        [Route("api/ea/ping")]
        [HttpPost]
        [Consumes("text/plain")]
        [RequireToken]
        public IActionResult PostPing()
        {
            return Ok();
        }

        [Route("api/ea/token/generate")]
        [HttpPost]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostToken([FromHeader(Name = "X-Ea-Type")] string eaType, [FromBody] string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("InvalidSecretKey");
            }
            if (string.IsNullOrEmpty(eaType) || !Enum.GetNames<SecretKeyType>().Contains(eaType))
            {
                return BadRequest("InvalidKeyType");
            }
            var type = Enum.Parse<SecretKeyType>(eaType);

            try
            {
                var secretKey = await mediator.Send(new FindBySecretKey(key));
                if (secretKey == null)
                {
                    await mediator.Send(new SecretKeyCreateIfNotExistsCommand(null, key, null));
                    return BadRequest("InvalidSecretKey");
                }
                else if (!secretKey.IsApproved)
                {
                    return BadRequest("InvalidSecretKey");
                }
                else if ((secretKey.KeyType & type) != type)
                {
                    return BadRequest("InvalidSecretKey");
                }
                else if (secretKey.IsConnected)
                {
                    return BadRequest("SecretKeyInUse");
                }
                var token = await mediator.Send(new SecretKeyGenerateSecretTokenCommand(secretKey.Id));
                return Ok(token);
            }
            catch
            {
                return BadRequest("SecretKeyInUse");
            }
        }

        [Route("api/ea/token/release")]
        [HttpPost]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostTokenRelease([FromHeader(Name = "X-Secret-Token")] string token)
        {
            var secretKey = await mediator.Send(new FindByCurrentToken(token));
            if (secretKey == null)
            {
                return BadRequest("InvalidToken");
            }
            await mediator.Send(new SecretKeyInvalidateSecretTokenCommand(secretKey.Id, token));
            return Ok(token);
        }

        [Route("api/ea/orders")]
        [HttpGet]
        [Produces("text/plain")]
        [RequireToken]
        [RequireKeyType("Subscriber")]
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

            await mediator.Send(new SubscriberFetchOrderCommand(Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), cacheEntry.Tickets));
            HttpContext.Response.Headers["ETag"] = $"\"{cacheEntry.Hash}\"";
            return Ok(cacheEntry.Text);
        }

        [Route("api/ea/orders")]
        [HttpPost]
        [Consumes("text/plain")]
        [RequireToken]
        [RequireKeyType("Publisher")]
        public async Task<IActionResult> PostOrders([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
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

            await mediator.Send(new PublisherReplaceOrdersCommand(Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value), orders));
            return Ok();
        }

        [Route("api/ea/logs")]
        [HttpPost]
        [Consumes("text/plain")]
        public IActionResult PostLogs([FromHeader(Name = "X-Ea-Id")] string key, [FromHeader(Name = "X-Ea-Type")] string keyType, [FromHeader(Name = "X-Ea-Version")] string version, [FromHeader(Name = "X-Secret-Token")] string token, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Accepted();
            }

            var logText = text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (!string.IsNullOrEmpty(key))
            {
                logger.LogInformation("Receive Log SecretKey:{secretKey}/Type:{type}/Version:{version}/Token:{token}, {@text}", key, keyType, version, token ?? "None", logText);
            }
            else
            {
                logger.LogInformation("Receive Log SecretKey:{secretKey}/Type:{type}/Version:{version}/Token:{token}, {@text}", User.Identity.Name ?? "Unknown", "Unknown", version ?? "Unknown", token ?? "None", logText);
            }
            return Accepted();
        }
    }
}
