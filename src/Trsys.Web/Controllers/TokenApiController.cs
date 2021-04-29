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

        public TokenApiController(SecretKeyService service, IAuthenticationTicketStore tokenStore)
        {
            this.service = service;
            this.ticketStore = tokenStore;
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
                    return BadRequest("InvalidSecretKey");
                }
            }
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
            await service.ReleaseSecretTokenAsync(ticket.Principal.Identity.Name);
            return Ok(token);
        }
    }
}
