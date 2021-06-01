using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;
using Trsys.Web.Filters;
using Trsys.Web.Models.ReadModel.Events;

namespace Trsys.Web.Controllers
{
    [Route("api/logs")]
    [EaVersion("20210331")]
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
        [RequireToken()]
        public async Task<IActionResult> PostLog([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Accepted();
            }

            foreach (var line in text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                await mediator.Publish(new EaEventNotification( User.Identity.Name, "Log", line));
            }
            return Accepted();
        }
    }
}
