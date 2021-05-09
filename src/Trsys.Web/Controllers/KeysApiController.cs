using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Trsys.Web.Models.SecretKeys;
using Trsys.Web.Services;

namespace Trsys.Web.Controllers
{
    [Route("api/keys")]
    [Authorize(Roles = "Administrator")]
    public class KeysApiController : Controller
    {
        private readonly SecretKeyService service;

        public KeysApiController(SecretKeyService service)
        {
            this.service = service;
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> PostKey([FromBody] PostKeyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await service.RegisterSecretKeyAsync(null, request.KeyType.Value, request.Description);
            if (!result.Success)
            {
                return BadRequest(new { result.ErrorMessage });
            }
            if (request.IsApproved)
            {
                await service.ApproveSecretKeyAsync(result.Key);
            }
            else
            {
                await service.RevokeSecretKeyAsync(result.Key);
            }
            return CreatedAtAction("GetKey", new { key = result.Key }, new { key = result.Key });
        }

        [HttpGet("{key}")]
        [Consumes("application/json")]
        public async Task<IActionResult> GetKey(string key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var data = await service.FindBySecretKeyAsync(key);
            return Ok(new SecretKeyEntity()
            {
                Key = data.Key,
                KeyType = data.KeyType,
                Description = data.Description,
                IsApproved = data.IsValid,
            });
        }

        [HttpPut("{key}")]
        [Consumes("application/json")]
        public async Task<IActionResult> PutKey(string key, PutKeyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var data = await service.FindBySecretKeyAsync(key);
            if (data == null)
            {
                return NotFound();
            }

            var result = await service.UpdateSecretKeyAsync(key, request.KeyType.Value, request.Description);
            if (!result.Success)
            {
                return BadRequest(new { result.ErrorMessage });
            }
            if (request.IsApproved)
            {
                await service.ApproveSecretKeyAsync(key);
            }
            else
            {
                await service.RevokeSecretKeyAsync(key);
            }
            return Ok();
        }

        [HttpDelete("{key}")]
        [Consumes("application/json")]
        public async Task<IActionResult> DeleteKey(string key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await service.DeleteSecretKeyAsync(key);
            if (!result.Success)
            {
                return BadRequest(new { result.ErrorMessage });
            }
            return Ok();
        }

        public class SecretKeyEntity
        {
            public string Key { get; set; }
            public SecretKeyType? KeyType { get; set; }
            public string Description { get; set; }
            public bool IsApproved { get; set; }
        }

        public class PostKeyRequest
        {
            [Required]
            public SecretKeyType? KeyType { get; set; }
            public string Description { get; set; }
            public bool IsApproved { get; set; }
        }

        public class PutKeyRequest
        {
            [Required]
            public SecretKeyType? KeyType { get; set; }
            public string Description { get; set; }
            public bool IsApproved { get; set; }
        }
    }
}
