using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trsys.Web.Filters;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Services;

namespace Trsys.Web.Controllers
{
    [Route("api/token")]
    [EaVersion("20210331")]
    [ApiController]
    public class TokenApiController : ControllerBase
    {

        private readonly IMediator mediator;
        private readonly EventService eventService;

        public TokenApiController(IMediator mediator, EventService eventService)
        {
            this.mediator = mediator;
            this.eventService = eventService;
        }

        [HttpPost]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostToken([FromBody] string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("InvalidSecretKey");
            }

            try
            {
                var secretKey = await mediator.Send(new FindBySecretKey(key));
                if (secretKey == null)
                {
                    await mediator.Send(new CreateSecretKeyCommand(null, key, null));
                    await eventService.RegisterSystemEventAsync("token", "NewEaAccessed", new { SecretKey = key });
                    return BadRequest("InvalidSecretKey");
                }
                else if (secretKey.IsValid)
                {
                    return BadRequest("InvalidSecretKey");
                }
                var token = await mediator.Send(new GenerateSecretTokenCommand(secretKey.Id));
                await eventService.RegisterSystemEventAsync("token", "TokenGenerated", new { SecretKey = key, SecretToken = token });
                return Ok(token);
            }
            catch
            {
                return BadRequest("SecretKeyInUse");
            }
        }

        [HttpPost("{token}/release")]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostTokenRelease(string token)
        {
            var secretKey = await mediator.Send(new FindByCurrentToken(token));
            if (secretKey == null)
            {
                return BadRequest("InvalidToken");
            }
            await mediator.Send(new InvalidateSecretTokenCommand(secretKey.Id));
            await eventService.RegisterSystemEventAsync("token", "TokenReleased", new { SecretKey = secretKey, SecretToken = token });
            return Ok(token);
        }
    }
}
