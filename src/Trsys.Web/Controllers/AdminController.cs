using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Authentication;
using Trsys.Web.Models.SecretKeys;
using Trsys.Web.Services;
using Trsys.Web.ViewModels.Admin;

namespace Trsys.Web.Controllers
{
    [Route("/admin")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly OrderService orderService;
        private readonly SecretKeyService secretKeyService;
        private readonly IAuthenticationTicketStore ticketStore;
        private readonly EventService eventService;

        public AdminController(
            OrderService orderService,
            SecretKeyService secretKeyService,
            IAuthenticationTicketStore ticketStore,
            EventService eventService)
        {
            this.orderService = orderService;
            this.secretKeyService = secretKeyService;
            this.ticketStore = ticketStore;
            this.eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = RestoreModel() ?? new IndexViewModel();
            if (TempData["KeyType"] != null)
            {
                model.KeyType = (SecretKeyType)TempData["KeyType"];
            }

            var order = await orderService.GetOrderTextEntryAsync();
            model.CacheOrderText = order?.Text;

            model.SecretKeys = (await secretKeyService.SearchAllAsync())
                .OrderBy(e => e.IsValid)
                .ThenBy(e => e.KeyType)
                .ThenBy(e => e.Id)
                .ToList();
            return View(model);
        }

        [HttpPost("orders/clear")]
        public async Task<IActionResult> PostOrdersClear(IndexViewModel model)
        {
            await orderService.ClearOrdersAsync();
            await eventService.RegisterUserEventAsync(User.Identity.Name, "OrderCleared");
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("keys/new")]
        public async Task<IActionResult> PostKeyNew(IndexViewModel model)
        {
            if (!model.KeyType.HasValue)
            {
                model.ErrorMessage = "キーの種類が指定されていません。";
                return SaveModelAndRedirectToIndex(model);
            }

            var result = await secretKeyService.RegisterSecretKeyAsync(model.Key, model.KeyType.Value, model.Description);
            if (!result.Success)
            {
                model.ErrorMessage = result.ErrorMessage;
                return SaveModelAndRedirectToIndex(model);
            }

            await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyRegistered", new { SecretKey = result.Key, model.KeyType, model.Description });
            model.SuccessMessage = $"シークレットキー: {result.Key} を作成しました。";
            model.KeyType = null;
            model.Key = null;
            model.Description = null;
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("keys/{id}/update")]
        public async Task<IActionResult> PostKeyUpdate(string id, IndexViewModel model)
        {
            id = System.Uri.UnescapeDataString(id);
            var updateRequest = model.SecretKeys.FirstOrDefault(sk => sk.Key == id);
            if (updateRequest == null || !updateRequest.KeyType.HasValue)
            {
                model.ErrorMessage = $"シークレットキー: {id} を編集できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            var result = await secretKeyService.UpdateSecretKeyAsync(id, updateRequest.KeyType.Value, updateRequest.Description);
            if (!result.Success)
            {
                model.ErrorMessage = result.ErrorMessage;
                return SaveModelAndRedirectToIndex(model);
            }

            await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyUpdated", new { SecretKey = id, updateRequest.KeyType, updateRequest.Description });
            model.SuccessMessage = $"シークレットキー: {id} を変更しました。";
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("keys/{id}/approve")]
        public async Task<IActionResult> PostKeyApprove(string id, IndexViewModel model)
        {
            id = System.Uri.UnescapeDataString(id);
            var result = await secretKeyService.ApproveSecretKeyAsync(id);
            if (!result.Success)
            {
                model.ErrorMessage = result.ErrorMessage;
                return SaveModelAndRedirectToIndex(model);
            }

            await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyApproved", new { SecretKey = id });
            model.SuccessMessage = $"シークレットキー: {id} を有効化しました。";
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("keys/{id}/revoke")]
        public async Task<IActionResult> PostKeyRevoke(string id, IndexViewModel model)
        {
            id = System.Uri.UnescapeDataString(id);
            var result = await secretKeyService.RevokeSecretKeyAsync(id);
            if (!result.Success)
            {
                model.ErrorMessage = result.ErrorMessage;
                return SaveModelAndRedirectToIndex(model);
            }

            await ticketStore.RemoveAsync(id);
            await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyRevoked", new { SecretKey = id });
            model.SuccessMessage = $"シークレットキー: {id} を無効化しました。";
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("keys/{id}/delete")]
        public async Task<IActionResult> PostKeyDelete(string id, IndexViewModel model)
        {
            id = System.Uri.UnescapeDataString(id);
            var result = await secretKeyService.DeleteSecretKeyAsync(id);
            if (!result.Success)
            {
                model.ErrorMessage = result.ErrorMessage;
                return SaveModelAndRedirectToIndex(model);
            }

            await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyDeleted", new { SecretKey = id });
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
