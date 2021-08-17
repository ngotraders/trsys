using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.ViewModels.Admin;

namespace Trsys.Web.Controllers
{
    [Route("/admin")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IMediator mediator;

        public AdminController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page, int? perPage)
        {
            var model = RestoreModel() ?? new IndexViewModel();
            if (TempData["KeyType"] != null)
            {
                model.KeyType = (SecretKeyType)TempData["KeyType"];
            }

            var order = await mediator.Send(new GetOrderTextEntry());
            model.CacheOrderText = order?.Text;
            var pagedResult = await mediator.Send(new GetSecretKeysWithPagination(page ?? 1, perPage ?? 20));
            model.SecretKeys = pagedResult.List;
            model.SecretKeysTotalCount = pagedResult.TotalCount;
            model.SecretKeysPage = pagedResult.Page;
            model.SecretKeysPerPage = pagedResult.PerPage;
            return View(model);
        }

        [HttpPost("orders/clear")]
        public async Task<IActionResult> PostOrdersClear(IndexViewModel model)
        {
            var orders = await mediator.Send(new GetOrders());
            foreach (var id in orders.Select(o => o.SecretKeyId).Distinct().ToList())
            {
                await mediator.Send(new ClearOrdersCommand(id));
            }
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

            try
            {
                var id = await mediator.Send(new CreateSecretKeyCommand(model.KeyType.Value, model.Key, model.Description));
                var result = await mediator.Send(new GetSecretKey(id));
                model.SuccessMessage = $"シークレットキー: {result.Key} を作成しました。";
                model.KeyType = null;
                model.Key = null;
                model.Description = null;
                return SaveModelAndRedirectToIndex(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return SaveModelAndRedirectToIndex(model);
            }
        }

        [HttpPost("keys/{id}/update")]
        public async Task<IActionResult> PostKeyUpdate([FromRoute] Guid id, IndexViewModel model)
        {
            try
            {
                var updateRequest = model.SecretKeys?.FirstOrDefault(sk => sk.Id == id);
                var secretKey = await mediator.Send(new GetSecretKey(id));
                if (secretKey == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                await mediator.Send(new UpdateSecretKeyCommand(secretKey.Id, updateRequest == null ? secretKey.KeyType : updateRequest.KeyType, updateRequest == null ? secretKey.Description : updateRequest.Description));
                model.SuccessMessage = $"シークレットキー: {secretKey.Key} を変更しました。";
                return SaveModelAndRedirectToIndex(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return SaveModelAndRedirectToIndex(model);
            }
        }

        [HttpPost("keys/{id}/approve")]
        public async Task<IActionResult> PostKeyApprove([FromRoute] Guid id, IndexViewModel model)
        {
            try
            {
                var updateRequest = model.SecretKeys?.FirstOrDefault(sk => sk.Id == id);
                var secretKey = await mediator.Send(new GetSecretKey(id));
                if (secretKey == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                await mediator.Send(new UpdateSecretKeyCommand(secretKey.Id, updateRequest == null ? secretKey.KeyType : updateRequest.KeyType, updateRequest == null ? secretKey.Description : updateRequest.Description, true));
                model.SuccessMessage = $"シークレットキー: {secretKey.Key} を有効化しました。";
                return SaveModelAndRedirectToIndex(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return SaveModelAndRedirectToIndex(model);
            }
        }

        [HttpPost("keys/{id}/revoke")]
        public async Task<IActionResult> PostKeyRevoke([FromRoute] Guid id, IndexViewModel model)
        {
            try
            {
                var updateRequest = model.SecretKeys?.FirstOrDefault(sk => sk.Id == id);
                var secretKey = await mediator.Send(new GetSecretKey(id));
                if (secretKey == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                var token = secretKey.Token;
                await mediator.Send(new UpdateSecretKeyCommand(secretKey.Id, updateRequest == null ? secretKey.KeyType : updateRequest.KeyType, updateRequest == null ? secretKey.Description : updateRequest.Description, false));
                model.SuccessMessage = $"シークレットキー: {secretKey.Key} を無効化しました。";
                return SaveModelAndRedirectToIndex(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return SaveModelAndRedirectToIndex(model);
            }
        }

        [HttpPost("keys/{id}/delete")]
        public async Task<IActionResult> PostKeyDelete([FromRoute] Guid id, IndexViewModel model)
        {
            try
            {
                var secretKey = await mediator.Send(new GetSecretKey(id));
                if (secretKey == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                await mediator.Send(new DeleteSecretKeyCommand(secretKey.Id));
                model.SuccessMessage = $"シークレットキー: {secretKey.Key} を削除しました。";
                return SaveModelAndRedirectToIndex(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return SaveModelAndRedirectToIndex(model);
            }
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
