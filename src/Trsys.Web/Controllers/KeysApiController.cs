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
        private readonly EventService eventService;

        public KeysApiController(SecretKeyService service, EventService eventService)
        {
            this.service = service;
            this.eventService = eventService;
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
            await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyRegistered", new { SecretKey = result.Key, request.KeyType.Value, request.Description });
            if (request.IsApproved)
            {
                await service.ApproveSecretKeyAsync(result.Key);
                await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyRegistered", new { result.Key });
            }
            await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyPosted", new { SecretKey = result.Key, request.KeyType, request.Description, request.IsApproved });
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

            await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyPutted", new { SecretKey = key, request.KeyType, request.Description, request.IsApproved });
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
            await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyDeleted", new { SecretKey = key });
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
