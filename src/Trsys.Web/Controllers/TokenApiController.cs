using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokenApiController : ControllerBase
    {
        private readonly ITokenGenerator generator;

        public TokenApiController(ITokenGenerator generator)
        {
            this.generator = generator;
        }

        [HttpPost]
        [Consumes("text/plain")]
        [Produces("text/plain")]
        public async Task<IActionResult> PostToken([FromBody] string secretKey)
        {
            var result = await generator.GenerateTokenAsync(secretKey);
            if (result.Succeeded)
            {
                return Ok(result.Token);
            }
            else
            {
                return BadRequest(Enum.GetName(result.Failure.Value.GetType(), result.Failure.Value));
            }
        }
    }
}
