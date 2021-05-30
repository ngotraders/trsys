using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Services;

namespace Trsys.Web.Controllers
{
    [Route("api/keys")]
    [Authorize(Roles = "Administrator")]
    public class KeysApiController : Controller
    {
        private readonly IMediator mediator;
        private readonly EventService eventService;

        public KeysApiController(IMediator mediator, EventService eventService)
        {
            this.mediator = mediator;
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
            try
            {
                var id = await mediator.Send(new CreateSecretKeyCommand(request.KeyType, request.Key, request.Description, request.IsApproved));
                var result = await mediator.Send(new GetSecretKey(id));
                await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyCreated", new { Id = id, SecretKey = result.Key, request.KeyType, request.Description, request.IsApproved });
                return CreatedAtAction("GetKey", new { id }, new { id });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> GetKey(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var secretKey = await mediator.Send(new GetSecretKey(id));
                if (secretKey == null)
                {
                    return NotFound();
                }
                return Ok(new SecretKeyEntity()
                {
                    Key = secretKey.Key,
                    KeyType = secretKey.KeyType,
                    Description = secretKey.Description,
                    IsApproved = secretKey.IsValid,
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> PutKey(Guid id, PutKeyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var secretKey = await mediator.Send(new GetSecretKey(id));
            var oldValid = secretKey.IsValid;
            if (secretKey == null)
            {
                return NotFound();
            }
            try
            {
                await mediator.Send(new UpdateSecretKeyCommand(id, request.KeyType, request.Description, request.IsApproved));
                var result = await mediator.Send(new GetSecretKey(id));
                await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyUpdated", new { Id = id, SecretKey = secretKey.Key, request.KeyType, request.Description, request.IsApproved });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpDelete("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> DeleteKey(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var secretKey = await mediator.Send(new GetSecretKey(id));
            if (secretKey == null)
            {
                return NotFound();
            }
            try
            {
                await mediator.Send(new DeleteSecretKeyCommand(id));
                await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyDeleted", new { Id = id, SecretKey = secretKey.Key });
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
