using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Authentication;
using Trsys.Web.Caching;
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
        private readonly IMemoryCache cache;

        public AdminController(ISecretKeyRepository secretKeyRepository, ISecretTokenStore tokenStore, IMemoryCache cache)
        {
            this.secretKeyRepository = secretKeyRepository;
            this.tokenStore = tokenStore;
            this.cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = RestoreModel() ?? new IndexViewModel();
            if (TempData["KeyType"] != null)
            {
                model.KeyType = (SecretKeyType)TempData["KeyType"];
            }

            if (cache.TryGetValue(CacheKeys.ORDERS_CACHE, out OrdersCache cacheEntry))
            {
                model.CacheOrderText = cacheEntry.Text;
            }
            model.SecretKeys = await secretKeyRepository
                .All
                .OrderBy(e => e.IsValid)
                .ThenBy(e => e.KeyType)
                .ThenBy(e => e.Id)
                .ToListAsync();
            return View(model);
        }

        [HttpPost("keys/new")]
        public async Task<IActionResult> PostKeyNew(IndexViewModel model)
        {
            if (!model.KeyType.HasValue)
            {
                model.ErrorMessage = "キーの種類が指定されていません。";
                return SaveModelAndRedirectToIndex(model);
            }

            SecretKey newSecretKey;
            if (string.IsNullOrEmpty(model.Key))
            {
                newSecretKey = await secretKeyRepository.CreateNewSecretKeyAsync(model.KeyType.Value);
            }
            else
            {
                newSecretKey = await secretKeyRepository.FindBySecretKeyAsync(model.Key);
                if (newSecretKey != null)
                {
                    model.ErrorMessage = "既に存在するキーです。";
                    return SaveModelAndRedirectToIndex(model);
                }
                newSecretKey = new SecretKey()
                {
                    Key = model.Key,
                    KeyType = model.KeyType,
                    Description = model.Description,
                };
            }
            await secretKeyRepository.SaveAsync(newSecretKey);
            model.SuccessMessage = $"シークレットキー: {newSecretKey.Key} を作成しました。";
            model.KeyType = null;
            model.Key = null;
            model.Description = null;
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("keys/{id}/update")]
        public async Task<IActionResult> PostKeyUpdate(string id, IndexViewModel model)
        {
            id = System.Uri.UnescapeDataString(id);
            var secretKey = await secretKeyRepository
                .FindBySecretKeyAsync(id);
            if (secretKey == null)
            {
                model.ErrorMessage = $"シークレットキー: {id} を編集できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            var updateRequest = model.SecretKeys.FirstOrDefault(sk => sk.Key == id);
            if (updateRequest == null || !updateRequest.KeyType.HasValue)
            {
                model.ErrorMessage = $"シークレットキー: {id} を編集できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            if (secretKey.IsValid && updateRequest.KeyType.Value != secretKey.KeyType.Value)
            {
                model.ErrorMessage = $"シークレットキー: {id} を編集できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            secretKey.KeyType = updateRequest.KeyType;
            secretKey.Description = updateRequest.Description;

            await secretKeyRepository.SaveAsync(secretKey);

            model.SuccessMessage = $"シークレットキー: {secretKey.Key} を変更しました。";
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("keys/{id}/approve")]
        public async Task<IActionResult> PostKeyApprove(string id, IndexViewModel model)
        {
            id = System.Uri.UnescapeDataString(id);
            var secretKey = await secretKeyRepository
                .FindBySecretKeyAsync(id);
            if (secretKey == null || secretKey.IsValid)
            {
                model.ErrorMessage = $"シークレットキー: {id} を有効化できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            secretKey.Approve();
            await secretKeyRepository.SaveAsync(secretKey);

            model.SuccessMessage = $"シークレットキー: {secretKey.Key} を有効化しました。";
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("keys/{id}/revoke")]
        public async Task<IActionResult> PostKeyRevoke(string id, IndexViewModel model)
        {
            id = System.Uri.UnescapeDataString(id);
            var secretKey = await secretKeyRepository
                .FindBySecretKeyAsync(id);
            if (secretKey == null || !secretKey.IsValid)
            {
                model.ErrorMessage = $"シークレットキー: {id} を無効化できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            var validToken = secretKey.ValidToken;
            secretKey.Revoke();
            await secretKeyRepository.SaveAsync(secretKey);

            // DBを更新した後にトークンを無効化する
            if (!string.IsNullOrEmpty(validToken))
            {
                await tokenStore.UnregisterAsync(validToken);
            }

            model.SuccessMessage = $"シークレットキー: {secretKey.Key} を無効化しました。";
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("keys/{id}/delete")]
        public async Task<IActionResult> PostKeyDelete(string id, IndexViewModel model)
        {
            id = System.Uri.UnescapeDataString(id);
            var secretKey = await secretKeyRepository
                .FindBySecretKeyAsync(id);
            if (secretKey == null || secretKey.IsValid)
            {
                TempData["ErrorMessage"] = $"シークレットキー: {id} を削除できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            await secretKeyRepository.RemoveAsync(secretKey);

            model.SuccessMessage = $"シークレットキー: {id} を削除しました。";
            return SaveModelAndRedirectToIndex(model);
        }

        private IndexViewModel RestoreModel()
        {
            var modelStr = TempData["Model"] as string;
            if (!string.IsNullOrEmpty(modelStr))
            {
                return JsonConvert.DeserializeObject<IndexViewModel>(modelStr);
            }
            return null;
        }

        private IActionResult SaveModelAndRedirectToIndex(IndexViewModel model)
        {
            TempData["Model"] = JsonConvert.SerializeObject(model); ;
            return RedirectToAction("Index");
        }

    }
}
