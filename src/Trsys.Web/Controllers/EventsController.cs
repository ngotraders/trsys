using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trsys.Web.Services;
using Trsys.Web.ViewModels.Events;

namespace Trsys.Web.Controllers
{
    [Route("/admin/events")]
    [Authorize(Roles = "Administrator")]
    public class EventsController : Controller
    {
        private readonly SecretKeyService secretKeyService;
        private readonly EventService eventService;

        public EventsController(SecretKeyService secretKeyService, EventService eventService)
        {
            this.secretKeyService = secretKeyService;
            this.eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string source, int? page, int? perPage)
        {
            var model = new IndexViewModel()
            {
                Page = page ?? 1,
                PerPage = perPage ?? 100,
                Source = source,
                SecretKeys = await secretKeyService.SearchAllAsync(),
            };

            model.Events = await eventService.SearchAsync(source, model.Page, model.PerPage);
            return View(model);
        }
    }
}
