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
        private readonly ILogger logger;

        public LogsApiController(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger("Trsys.Web.Ea");
        }

        [HttpPost]
        [Consumes("text/plain")]
        public IActionResult PostLog([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Accepted();
            }

            var secretKey = (string)HttpContext.Request.Headers["X-Ea-Id"];
            var type = (string)HttpContext.Request.Headers["X-Ea-Type"];
            var version = (string)HttpContext.Request.Headers["X-Ea-Version"] ?? (string)HttpContext.Request.Headers["Version"];
            var logText = text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (string.IsNullOrEmpty(secretKey))
            {
                logger.LogDebug("Receive Log SecretKey:{secretKey}/Type:{type}/Version:{version}, {@text}", secretKey, type, version, logText);
            }
            else
            {
                logger.LogDebug("Receive Log SecretKey:{secretKey}/Type:{type}/Version:{version}, {@text}", User.Identity.Name, "Unknown", version ?? "Unknown", logText);
            }
            return Accepted();
        }
    }
}
