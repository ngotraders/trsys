using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Events;
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
        public async Task<IActionResult> Index()
        {
            var model = RestoreModel() ?? new IndexViewModel();
            if (TempData["KeyType"] != null)
            {
                model.KeyType = (SecretKeyType)TempData["KeyType"];
            }

            var order = await mediator.Send(new GetOrderTextEntry());
            model.CacheOrderText = order?.Text;

            model.SecretKeys = (await mediator.Send(new GetSecretKeys()))
                .OrderBy(e => e.IsApproved)
                .ThenBy(e => e.KeyType)
                .ThenBy(e => e.Id)
                .ToList();
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
            await mediator.Publish(new UserEventNotification(User.Identity.Name, "OrderCleared"));
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
                await mediator.Publish(new UserEventNotification(User.Identity.Name, "SecretKeyRegistered", new { Id = id, SecretKey = result.Key, result.KeyType, result.Description }));
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

        [HttpPost("keys/{key}/update")]
        public async Task<IActionResult> PostKeyUpdate([FromRoute] string key, IndexViewModel model)
        {

            try
            {
                var updateRequest = model.SecretKeys?.FirstOrDefault(sk => sk.Key == key);
                var secretKey = await mediator.Send(new FindBySecretKey(key));
                if (secretKey == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                await mediator.Send(new UpdateSecretKeyCommand(secretKey.Id, updateRequest == null ? secretKey.KeyType : updateRequest.KeyType, updateRequest == null ? secretKey.Description : updateRequest.Description));
                var result = await mediator.Send(new GetSecretKey(secretKey.Id));
                await mediator.Publish(new UserEventNotification(User.Identity.Name, "SecretKeyUpdated", new { SecretKey = key, result.KeyType, result.Description }));
                model.SuccessMessage = $"シークレットキー: {key} を変更しました。";
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

        [HttpPost("keys/{key}/approve")]
        public async Task<IActionResult> PostKeyApprove([FromRoute] string key, IndexViewModel model)
        {
            try
            {
                var updateRequest = model.SecretKeys?.FirstOrDefault(sk => sk.Key == key);
                var secretKey = await mediator.Send(new FindBySecretKey(key));
                if (secretKey == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                await mediator.Send(new UpdateSecretKeyCommand(secretKey.Id, updateRequest == null ? secretKey.KeyType : updateRequest.KeyType, updateRequest == null ? secretKey.Description : updateRequest.Description, true));
                await mediator.Publish(new UserEventNotification(User.Identity.Name, "SecretKeyApproved", new { SecretKey = key }));
                model.SuccessMessage = $"シークレットキー: {key} を有効化しました。";
                return SaveModelAndRedirectToIndex(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return SaveModelAndRedirectToIndex(model);
            }
        }

        [HttpPost("keys/{key}/revoke")]
        public async Task<IActionResult> PostKeyRevoke([FromRoute] string key, IndexViewModel model)
        {
            try
            {
                var updateRequest = model.SecretKeys?.FirstOrDefault(sk => sk.Key == key);
                var secretKey = await mediator.Send(new FindBySecretKey(key));
                if (secretKey == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                var token = secretKey.Token;
                await mediator.Send(new UpdateSecretKeyCommand(secretKey.Id, updateRequest == null ? secretKey.KeyType : updateRequest.KeyType, updateRequest == null ? secretKey.Description : updateRequest.Description, false));
                if (!string.IsNullOrEmpty(token))
                {
                    await mediator.Publish(new UserEventNotification(User.Identity.Name, "TokenInvalidated", new { SecretKey = key, Token = token }));
                }
                await mediator.Publish(new UserEventNotification(User.Identity.Name, "SecretKeyRevoked", new { SecretKey = key }));
                model.SuccessMessage = $"シークレットキー: {key} を無効化しました。";
                return SaveModelAndRedirectToIndex(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return SaveModelAndRedirectToIndex(model);
            }
        }

        [HttpPost("keys/{key}/delete")]
        public async Task<IActionResult> PostKeyDelete([FromRoute] string key, IndexViewModel model)
        {
            try
            {
                var secretKey = await mediator.Send(new FindBySecretKey(key));
                if (secretKey == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                await mediator.Send(new DeleteSecretKeyCommand(secretKey.Id));
                await mediator.Publish(new UserEventNotification(User.Identity.Name, "SecretKeyDeleted", new { SecretKey = key }));
                model.SuccessMessage = $"シークレットキー: {key} を削除しました。";
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
