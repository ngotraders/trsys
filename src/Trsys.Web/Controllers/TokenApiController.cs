using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trsys.Web.Filters;
using Trsys.Web.Services;

namespace Trsys.Web.Controllers
{
    [Route("api/token")]
    [EaVersion("20210331")]
    [ApiController]
    public class TokenApiController : ControllerBase
    {

        private readonly SecretKeyService service;
        private readonly EventService eventService;

        public TokenApiController(SecretKeyService service, EventService eventService)
        {
            this.service = service;
            this.eventService = eventService;
        }

        [HttpPost]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostToken([FromBody] string secretKey)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                return BadRequest("InvalidSecretKey");
            }

            var result = await service.GenerateSecretTokenAsync(secretKey);
            if (!result.Success)
            {
                if (result.InUse)
                {
                    return BadRequest("SecretKeyInUse");
                }
                else
                {
                    if (result.NewlyCreated)
                    {
                        await eventService.RegisterSystemEventAsync("token", "NewEaAccessed", new { SecretKey = secretKey });
                    }
                    return BadRequest("InvalidSecretKey");
                }
            }

            await eventService.RegisterSystemEventAsync("token", "TokenGenerated", new { SecretKey = secretKey, SecretToken = result.Token });
            return Ok(result.Token);
        }

        [HttpPost("{token}/release")]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostTokenRelease(string token)
        {
            var result = await service.ReleaseSecretTokenAsync(token) as ReleaseSecretTokenResult;
            if (result == null)
            {
                return BadRequest("InvalidToken");
            }
            await eventService.RegisterSystemEventAsync("token", "TokenReleased", new { SecretKey = result.Key, SecretToken = token });
            return Ok(token);
        }
    }
}
