using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Auth;
using Trsys.Web.Models;
using Trsys.Web.ViewModels.Admin;

namespace Trsys.Web.Controllers
{
    [Route("/admin")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ISecretKeyRepository secretKeyRepository;
        private readonly ISecretTokenStore tokenStore;

        public AdminController(ISecretKeyRepository secretKeyRepository, ISecretTokenStore tokenStore)
        {
            this.secretKeyRepository = secretKeyRepository;
            this.tokenStore = tokenStore;
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
        public async Task<IActionResult> PostKeyNew(PostNewKeyRequest request)
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

        [HttpPost("keys/{id}/approve")]
        public async Task<IActionResult> PostKeyApprove(string id)
        {
            var secretKey = await secretKeyRepository
                .FindBySecretKeyAsync(id);
            if (secretKey == null || secretKey.IsValid)
            {
                TempData["ErrorMessage"] = $"シークレットキー: {id} を有効化できません。";
                return RedirectToAction("Index");
            }

            secretKey.Approve();
            await secretKeyRepository.SaveAsync(secretKey);

            TempData["SuccessMessage"] = $"シークレットキー: {secretKey.Key} を有効化しました。";
            return RedirectToAction("Index");
        }

        [HttpPost("keys/{id}/revoke")]
        public async Task<IActionResult> PostKeyRevoke(string id)
        {
            var secretKey = await secretKeyRepository
                .FindBySecretKeyAsync(id);
            if (secretKey == null || !secretKey.IsValid)
            {
                TempData["ErrorMessage"] = $"シークレットキー: {id} を無効化できません。";
                return RedirectToAction("Index");
            }

            var validToken = secretKey.ValidToken;
            secretKey.Revoke();
            await secretKeyRepository.SaveAsync(secretKey);

            // DBを更新した後にトークンを無効化する
            if (!string.IsNullOrEmpty(validToken))
            {
                await tokenStore.UnregisterAsync(validToken);
            }

            TempData["SuccessMessage"] = $"シークレットキー: {secretKey.Key} を無効化しました。";
            return RedirectToAction("Index");
        }

        [HttpPost("keys/{id}/delete")]
        public async Task<IActionResult> PostKeyDelete(string id)
        {
            var secretKey = await secretKeyRepository
                .FindBySecretKeyAsync(id);
            if (secretKey == null || secretKey.IsValid)
            {
                TempData["ErrorMessage"] = $"シークレットキー: {id} を削除できません。";
                return RedirectToAction("Index");
            }

            await secretKeyRepository.RemoveAsync(secretKey);

            TempData["SuccessMessage"] = $"シークレットキー: {id} を削除しました。";
            return RedirectToAction("Index");
        }

    }
}
