using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.ViewModels.Admin;

namespace Trsys.Web.Controllers
{
    [Route("/admin")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ISecretKeyRepository secretKeyRepository;

        public AdminController(ISecretKeyRepository secretKeyRepository)
        {
            this.secretKeyRepository = secretKeyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel();
            model.SecretKeys = await secretKeyRepository
                .All
                .OrderBy(e => e.Id)
                .ToListAsync();
            return View(model);
        }
    }
}
