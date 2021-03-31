using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trsys.Web.Authentication;
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
        public async Task<IActionResult> PostToken([FromBody] string secretKey)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                return BadRequest("InvalidSecretKey");
            }

            var secretKeyEntity = await repository.FindBySecretKeyAsync(secretKey);
            if (secretKeyEntity == null || !secretKeyEntity.IsValid || !secretKeyEntity.KeyType.HasValue)
            {
                if (secretKeyEntity == null)
                {
                    secretKeyEntity = new SecretKey()
                    {
                        Key = secretKey,
                    };
                    await repository.SaveAsync(secretKeyEntity);
                }
                return BadRequest("InvalidSecretKey");
            }

            if (!string.IsNullOrEmpty(secretKeyEntity.ValidToken))
            {
                var tokenInfo = await tokenStore.FindInfoAsync(secretKeyEntity.ValidToken);
                if (tokenInfo != null)
                {
                    if (tokenInfo.IsInUse())
                    {
                        return BadRequest("SecretKeyInUse");
                    }
                    await tokenStore.UnregisterAsync(secretKeyEntity.ValidToken);
                }
            }

            var token = await tokenStore.RegisterTokenAsync(secretKeyEntity.Key, secretKeyEntity.KeyType.Value);
            secretKeyEntity.UpdateToken(token);
            await repository.SaveAsync(secretKeyEntity);
            return Ok(token);
        }

        [HttpPost("{token}/release")]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostTokenRelease(string token)
        {
            var tokenInfo = await tokenStore.FindInfoAsync(token);
            if (tokenInfo == null)
            {
                return BadRequest("InvalidToken");
            }

            await tokenStore.UnregisterAsync(token);
            var secretKey = await repository.FindBySecretKeyAsync(tokenInfo.SecretKey);
            if (secretKey != null)
            {
                secretKey.ReleaseToken();
                await repository.SaveAsync(secretKey);
            }
            return Ok(token);
        }
    }
}
