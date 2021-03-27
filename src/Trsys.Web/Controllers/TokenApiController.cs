using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trsys.Web.Auth;
using Trsys.Web.Models;

namespace Trsys.Web.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokenApiController : ControllerBase
    {
        private readonly ISecretKeyRepository repository;
        private readonly ISecretTokenStore tokenStore;

        public TokenApiController(ISecretKeyRepository repository, ISecretTokenStore tokenStore)
        {
            this.repository = repository;
            this.tokenStore = tokenStore;
        }

        [HttpPost]
        [Consumes("text/plain")]
        [Produces("text/plain")]
        public async Task<IActionResult> PostToken([FromBody] string secretKey)
        {
            var result = await repository.FindBySecretKeyAsync(secretKey);
            if (result == null || result.IsValid)
            {
                return BadRequest("InvalidSecretKey");
            }

            if (!string.IsNullOrEmpty(result.ValidToken))
            {
                var tokenInfo = await tokenStore.FindInfoAsync(result.ValidToken);
                if (tokenInfo != null)
                {
                    if (tokenInfo.IsInUse())
                    {
                        return BadRequest("SecretKeyInUse");
                    }
                    await tokenStore.UnregisterAsync(result.ValidToken);
                }
            }

            var token = await tokenStore.RegisterTokenAsync(result.Key, result.KeyType);
            result.UpdateToken(token);
            await repository.SaveAsync(result);
            return Ok(token);
        }
    }
}
