using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;
using Trsys.Web.Filters;
using Trsys.Web.Services;

namespace Trsys.Web.Controllers
{
    [Route("api/logs")]
    [EaVersion("20210331")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "SecretToken")]
    public class LogsApiController : Controller
    {
        private readonly EventService service;

        public LogsApiController(EventService service)
        {
            this.service = service;
        }

        [HttpPost]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostLog([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Accepted();
            }

            foreach (var line in text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                await service.RegisterEaEventAsync(User.Identity.Name, "Log", line);
            }
            return Accepted();
        }
    }
}
