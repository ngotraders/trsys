using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using Trsys.Web.Services;

namespace Trsys.Web.Controllers
{
    [Route("api/logs")]
    [ApiController]
    public class LogsApiController : Controller
    {
        private readonly EventService service;

        public LogsApiController(EventService service)
        {
            this.service = service;
        }

        [HttpPost]
        [Consumes("text/plain")]
        [Authorize(AuthenticationSchemes = "SecretToken")]
        public async Task<IActionResult> PostLog([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string text)
        {
            await service.RegisterEaEventAsync(User.Identity.Name, "Log", text);
            return Accepted();
        }
    }
}
