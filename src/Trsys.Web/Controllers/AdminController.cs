using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
            model.ErrorMessage = TempData["ErrorMessage"] as string;
            model.SuccessMessage = TempData["SuccessMessage"] as string;
            model.SecretKeys = await secretKeyRepository
                .All
                .OrderBy(e => e.KeyType)
                .ThenBy(e => e.Id)
                .ToListAsync();
            return View(model);
        }

        [HttpPost("keys/new")]
        public async Task<IActionResult> PostNewKey(PostNewKeyRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "キーの種類が指定されていません。";
                return RedirectToAction("Index");
            }
            var newSecretKey = await secretKeyRepository
                .CreateNewSecretKeyAsync(request.KeyType.Value);
            await secretKeyRepository.SaveAsync(newSecretKey);
            TempData["SuccessMessage"] = $"シークレットキー: {newSecretKey.Key} を作成しました。";
            return RedirectToAction("Index");
        }

        public class PostNewKeyRequest
        {
            [Required]
            public SecretKeyType? KeyType { get; set; }
        }

    }
}
