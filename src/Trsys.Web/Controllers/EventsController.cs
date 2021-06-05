using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.SqlStreamStore;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.ViewModels.Events;

namespace Trsys.Web.Controllers
{
    [Route("/admin/events")]
    [Authorize(Roles = "Administrator")]
    public class EventsController : Controller
    {
        private readonly IMediator mediator;

        public EventsController(IMediator mediator)
        {
            this.mediator = mediator;
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

            model.Events = (await mediator.Send(new GetEvents(source, model.Page, model.PerPage))).ToList();
            return View(model);
        }
    }
}
