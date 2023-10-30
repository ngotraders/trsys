using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Web.Controllers
{
    [Route("api/keys")]
    [Authorize(Roles = "Administrator")]
    public class KeysApiController : Controller
    {
        private readonly IMediator mediator;

        public KeysApiController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> GetKeys()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                return Ok(await mediator.Send(new GetSecretKeys()));
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
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
            try
            {
                var id = await mediator.Send(new CreateSecretKeyCommand(request.KeyType, request.Key, request.Description, request.IsApproved));
                var result = await mediator.Send(new GetSecretKey(id));
                return CreatedAtAction("GetKey", new { key = result.Key }, new { key = result.Key });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("{key}")]
        [Consumes("application/json")]
        public async Task<IActionResult> GetKey(string key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var secretKey = await mediator.Send(new FindBySecretKey(key));
                if (secretKey == null)
                {
                    return NotFound();
                }
                return Ok(new SecretKeyEntity()
                {
                    Key = secretKey.Key,
                    KeyType = secretKey.KeyType,
                    Description = secretKey.Description,
                    IsApproved = secretKey.IsApproved,
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPut("{key}")]
        [Consumes("application/json")]
        public async Task<IActionResult> PutKey(string key, [FromBody] PutKeyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var secretKey = await mediator.Send(new FindBySecretKey(key));
            if (secretKey == null)
            {
                return NotFound();
            }
            try
            {
                await mediator.Send(new UpdateSecretKeyCommand(secretKey.Id, request.KeyType, request.Description, request.IsApproved));
                var result = await mediator.Send(new GetSecretKey(secretKey.Id));
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteKey(string key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var secretKey = await mediator.Send(new FindBySecretKey(key));
            if (secretKey == null)
            {
                return NotFound();
            }
            try
            {
                await mediator.Send(new DeleteSecretKeyCommand(secretKey.Id));
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
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
            public string Key { get; set; }
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
