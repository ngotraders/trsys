using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.ViewModels.Logs;

namespace Trsys.Web.Controllers
{
    [Route("/admin/logs")]
    [Authorize(Roles = "Administrator")]
    public class LogsController : Controller
    {
        private readonly TrsysContext db;

        public LogsController(TrsysContext db)
        {
            this.db = db;
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

            model.Events = await SearchAsync(source, page ?? 1, perPage ?? 100);
            return View(model);
        }

        private async Task<List<Log>> SearchAsync(string source, int page, int perPage)
        {
            var query = db.Logs.OrderByDescending(q => q.TimeStamp) as IQueryable<Log>;
            if (!string.IsNullOrEmpty(source))
            {
                query = query.Where(q => q.Level == source);
            }
            if (page > 1)
            {
                query = query.Skip((page - 1) * perPage);
            }
            if (perPage > 0)
            {
                query = query.Take(perPage);
            }
            return await query.ToListAsync();
        }
    }
}
