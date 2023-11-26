using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;
using Trsys.Web.ViewModels.Admin;

namespace Trsys.Web.Controllers
{
    [Route("/admin")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IMediator mediator;
        private readonly IConfiguration configuration;

        public AdminController(IMediator mediator, IConfiguration configuration)
        {
            this.mediator = mediator;
            this.configuration = configuration;
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
            model.EaSiteUrl = configuration.GetValue<string>("Trsys.Web:EaSiteUrl");
            return View(model);
        }

        [HttpPost("orders/new")]
        public async Task<IActionResult> PostOrderNew(IndexViewModel model)
        {
            if (string.IsNullOrEmpty(model.NewOrderSecretKey))
            {
                model.ErrorMessage = "シークレットキーが指定されていません。";
                return SaveModelAndRedirectToIndex(model);
            }
            var secretKey = await mediator.Send(new FindBySecretKey(model.NewOrderSecretKey));
            if (secretKey == null || (secretKey.KeyType & SecretKeyType.Publisher) != SecretKeyType.Publisher)
            {
                model.ErrorMessage = "シークレットキーが不正です。";
                return SaveModelAndRedirectToIndex(model);
            }
            if (string.IsNullOrEmpty(model.NewOrderSymbol))
            {
                model.ErrorMessage = "通貨ペアが指定されていません。";
                return SaveModelAndRedirectToIndex(model);
            }
            if (!model.NewOrderType.HasValue || (model.NewOrderType.Value != OrderType.Sell && model.NewOrderType.Value != OrderType.Buy))
            {
                model.ErrorMessage = "取引が指定されていません。";
                return SaveModelAndRedirectToIndex(model);
            }
            if (!model.NewOrderPrice.HasValue)
            {
                model.ErrorMessage = "価格が指定されていません。";
                return SaveModelAndRedirectToIndex(model);
            }
            var ticketNo = model.NewOrderTicketNo ?? Random.Shared.Next(1, short.MaxValue);
            await mediator.Send(new PublisherOpenOrderCommand(secretKey.Id, new PublishedOrder()
            {
                TicketNo = ticketNo,
                Symbol = model.NewOrderSymbol,
                OrderType = model.NewOrderType.Value,
                Time = (model.NewOrderTime ?? DateTimeOffset.Now).ToUnixTimeSeconds(),
                Price = model.NewOrderPrice.Value,
                Percentage = model.NewOrderPercentage.GetValueOrDefault(98),
            }));
            model.SuccessMessage = $"注文{ticketNo}を作成しました。";
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("orders/close")]
        public async Task<IActionResult> PostOrderClose(IndexViewModel model)
        {
            if (string.IsNullOrEmpty(model.CloseOrderSecretKey))
            {
                model.ErrorMessage = "シークレットキーが指定されていません。";
                return SaveModelAndRedirectToIndex(model);
            }
            var secretKey = await mediator.Send(new FindBySecretKey(model.CloseOrderSecretKey));
            if (secretKey == null || (secretKey.KeyType & SecretKeyType.Publisher) != SecretKeyType.Publisher)
            {
                model.ErrorMessage = "シークレットキーが不正です。";
                return SaveModelAndRedirectToIndex(model);
            }
            if (!model.CloseOrderTicketNo.HasValue)
            {
                model.ErrorMessage = "チケットNoが指定されていません。";
                return SaveModelAndRedirectToIndex(model);
            }
            await mediator.Send(new PublisherCloseOrderCommand(secretKey.Id, model.CloseOrderTicketNo.Value));
            model.SuccessMessage = $"注文{model.CloseOrderTicketNo.Value}を削除しました。";
            return SaveModelAndRedirectToIndex(model);
        }

        [HttpPost("orders/clear")]
        public async Task<IActionResult> PostOrdersClear(IndexViewModel model)
        {
            var orders = await mediator.Send(new GetOrders());
            foreach (var id in orders.Select(o => o.SecretKeyId).Distinct().ToList())
            {
                await mediator.Send(new PublisherClearOrdersCommand(id));
            }
            model.SuccessMessage = $"注文をクリアしました。";
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
                var id = await mediator.Send(new SecretKeyCreateCommand(model.KeyType.Value, model.Key, model.Description));
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
                await mediator.Send(new SecretKeyUpdateCommand(secretKey.Id, updateRequest == null ? secretKey.KeyType : updateRequest.KeyType, updateRequest == null ? secretKey.Description : updateRequest.Description));
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
                await mediator.Send(new SecretKeyUpdateCommand(secretKey.Id, updateRequest == null ? secretKey.KeyType : updateRequest.KeyType, updateRequest == null ? secretKey.Description : updateRequest.Description, true));
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
                await mediator.Send(new SecretKeyUpdateCommand(secretKey.Id, updateRequest == null ? secretKey.KeyType : updateRequest.KeyType, updateRequest == null ? secretKey.Description : updateRequest.Description, false));
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
                await mediator.Send(new SecretKeyDeleteCommand(secretKey.Id));
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
