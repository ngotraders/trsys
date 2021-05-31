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
using Trsys.Web.Services;
using Trsys.Web.ViewModels.Admin;

namespace Trsys.Web.Controllers
{
    [Route("/admin")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IMediator mediator;
        private readonly OrderService orderService;
        private readonly EventService eventService;

        public AdminController(
            IMediator mediator,
            OrderService orderService,
            EventService eventService)
        {
            this.mediator = mediator;
            this.orderService = orderService;
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

            model.SecretKeys = (await mediator.Send(new GetSecretKeys()))
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

            try
            {
                var id = await mediator.Send(new CreateSecretKeyCommand(model.KeyType.Value, model.Key, model.Description));
                var result = await mediator.Send(new GetSecretKey(id));
                await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyRegistered", new { Id = id, SecretKey = result.Key, result.KeyType, result.Description });
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
        public async Task<IActionResult> PostKeyUpdate(Guid id, IndexViewModel model)
        {
            var updateRequest = model.SecretKeys?.FirstOrDefault(sk => sk.Id == id);
            if (updateRequest == null || !updateRequest.KeyType.HasValue)
            {
                model.ErrorMessage = $"シークレットキーを変更できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            try
            {
                await mediator.Send(new UpdateSecretKeyCommand(id, updateRequest.KeyType.Value, updateRequest.Description));
                var result = await mediator.Send(new GetSecretKey(id));
                await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyUpdated", new { Id = id, SecretKey = result.Key, result.KeyType, result.Description });
                model.SuccessMessage = $"シークレットキー: {result.Key} を変更しました。";
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

        [HttpPost("keys/{id}/approve")]
        public async Task<IActionResult> PostKeyApprove(Guid id, IndexViewModel model)
        {
            var updateRequest = model.SecretKeys?.FirstOrDefault(sk => sk.Id == id);
            if (updateRequest == null)
            {
                model.ErrorMessage = $"シークレットキーを変更できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            try
            {
                var result = await mediator.Send(new GetSecretKey(id));
                if (result == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                await mediator.Send(new UpdateSecretKeyCommand(id, updateRequest.KeyType, updateRequest.Description, true));
                await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyApproved", new { Id = id, SecretKey = result.Key });
                model.SuccessMessage = $"シークレットキー: {result.Key} を有効化しました。";
                return SaveModelAndRedirectToIndex(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return SaveModelAndRedirectToIndex(model);
            }
        }

        [HttpPost("keys/{id}/revoke")]
        public async Task<IActionResult> PostKeyRevoke(Guid id, IndexViewModel model)
        {
            var updateRequest = model.SecretKeys?.FirstOrDefault(sk => sk.Id == id);
            if (updateRequest == null)
            {
                model.ErrorMessage = $"シークレットキーを変更できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            try
            {
                var result = await mediator.Send(new GetSecretKey(id));
                if (result == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                var token = result.Token;
                await mediator.Send(new UpdateSecretKeyCommand(id, updateRequest.KeyType, updateRequest.Description, false));
                if (!string.IsNullOrEmpty(token))
                {
                    await eventService.RegisterUserEventAsync(User.Identity.Name, "TokenInvalidated", new { Id = id, SecretKey = result.Key, Token = token });
                }
                await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyRevoked", new { Id = id, SecretKey = result.Key });
                model.SuccessMessage = $"シークレットキー: {result.Key} を無効化しました。";
                return SaveModelAndRedirectToIndex(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return SaveModelAndRedirectToIndex(model);
            }
        }

        [HttpPost("keys/{id}/delete")]
        public async Task<IActionResult> PostKeyDelete(Guid id, IndexViewModel model)
        {
            var updateRequest = model.SecretKeys.FirstOrDefault(sk => sk.Id == id);
            if (updateRequest == null)
            {
                model.ErrorMessage = $"シークレットキーを削除できません。";
                return SaveModelAndRedirectToIndex(model);
            }

            try
            {
                var result = await mediator.Send(new GetSecretKey(id));
                if (result == null)
                {
                    model.ErrorMessage = $"シークレットキーを変更できません。";
                    return SaveModelAndRedirectToIndex(model);
                }
                await mediator.Send(new DeleteSecretKeyCommand(id));
                await eventService.RegisterUserEventAsync(User.Identity.Name, "SecretKeyDeleted", new { Id = id, SecretKey = result.Key });
                model.SuccessMessage = $"シークレットキー: {result.Key} を削除しました。";
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
