using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using Trsys.Web.Filters;

namespace Trsys.Web.Controllers
{
    [Route("api/logs")]
    [EaEndpoint]
    [MinimumEaVersion("20210331")]
    [ApiController]
    public class LogsApiController : Controller
    {
        private readonly ILogger<LogsApiController> logger;

        public LogsApiController(ILogger<LogsApiController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [Consumes("text/plain")]
        public IActionResult PostLog([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Accepted();
            }

            var key = (string)HttpContext.Request.Headers["X-Ea-Id"];
            var type = (string)HttpContext.Request.Headers["X-Ea-Type"];
            var version = (string)HttpContext.Request.Headers["X-Ea-Version"] ?? (string)HttpContext.Request.Headers["Version"];
            var logText = text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (!string.IsNullOrEmpty(key))
            {
                logger.LogInformation("Receive Log SecretKey:{secretKey}/Type:{type}/Version:{version}, {@text}", key, type, version, logText);
            }
            else
            {
                logger.LogInformation("Receive Log SecretKey:{secretKey}/Type:{type}/Version:{version}, {@text}", User.Identity.Name ?? "Unknown", "Unknown", version ?? "Unknown", logText);
            }
            return Accepted();
        }
    }
}
