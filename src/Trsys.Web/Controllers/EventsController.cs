using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Services;
using Trsys.Web.ViewModels.Events;

namespace Trsys.Web.Controllers
{
    [Route("/admin/events")]
    [Authorize(Roles = "Administrator")]
    public class EventsController : Controller
    {
        private readonly IMediator mediator;
        private readonly EventService eventService;

        public EventsController(IMediator mediator, EventService eventService)
        {
            this.mediator = mediator;
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
                SecretKeys = await mediator.Send(new GetSecretKeys()),
            };

            model.Events = await eventService.SearchAsync(source, model.Page, model.PerPage);
            return View(model);
        }
    }
}
