using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;
using Trsys.Web.Filters;
using Trsys.Web.Models.Messaging;
using Trsys.Web.Models.ReadModel.Notifications;

namespace Trsys.Web.Controllers
{
    [Route("api/logs")]
    [EaEndpoint]
    [MinimumEaVersion("20210331")]
    [ApiController]
    public class LogsApiController : Controller
    {
        private readonly IMediator mediator;

        public LogsApiController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostLog([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Accepted();
            }

            var version = (string)HttpContext.Request.Headers["X-Ea-Version"] ?? (string)HttpContext.Request.Headers["Version"];
            await mediator.Publish(PublishingMessageEnvelope.Create(new LogNotification(HttpContext.TraceIdentifier, User.Identity.Name, version ?? "UNKNOWN", text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))));
            return Accepted();
        }
    }
}
