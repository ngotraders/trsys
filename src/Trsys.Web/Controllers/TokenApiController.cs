using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trsys.Web.Authentication;
using Trsys.Web.Services;

namespace Trsys.Web.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokenApiController : ControllerBase
    {

        private readonly SecretKeyService service;
        private readonly IAuthenticationTicketStore ticketStore;
        private readonly EventService eventService;

        public TokenApiController(SecretKeyService service, IAuthenticationTicketStore ticketStore, EventService eventService)
        {
            this.service = service;
            this.ticketStore = ticketStore;
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
                        await eventService.RegisterSystemEventAsync("NewEaAccessed", new { SecretKey = secretKey });
                    }
                    return BadRequest("InvalidSecretKey");
                }
            }

            await eventService.RegisterSystemEventAsync("TokenGenerated", new { SecretKey = secretKey, SecretToken = result.Token });
            var principal = SecretKeyAuthenticationTicketFactory.Create(result.Key, result.KeyType);
            await ticketStore.AddAsync(result.Token, principal);
            return Ok(result.Token);
        }

        [HttpPost("{token}/release")]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostTokenRelease(string token)
        {
            var ticket = await ticketStore.RemoveAsync(token);
            if (ticket == null)
            {
                return BadRequest("InvalidToken");
            }

            var secretKey = ticket.Principal.Identity.Name;
            await service.ReleaseSecretTokenAsync(secretKey);
            await eventService.RegisterSystemEventAsync("TokenReleased", new { SecretKey = secretKey, SecretToken = token });
            return Ok(token);
        }
    }
}
