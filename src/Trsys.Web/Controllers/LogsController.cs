using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.ViewModels.Logs;

namespace Trsys.Web.Controllers
{
    [Route("/admin/logs")]
    [Authorize(Roles = "Administrator")]
    public class LogsController : Controller
    {
        private readonly IMediator mediator;

        public LogsController(IMediator mediator)
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
            };

            model.Events = (await mediator.Send(new GetLogs(source, model.Page, model.PerPage))).ToList();
            return View(model);
        }
    }
}
