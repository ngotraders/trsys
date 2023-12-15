using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Notifications;

namespace Trsys.Web.Filters
{
    public class RequireTokenAttribute : ActionFilterAttribute
    {
        public SecretKeyType? KeyType { get; }

        public RequireTokenAttribute(string keyType = null)
        {
            if (!string.IsNullOrEmpty(keyType))
            {
                if (keyType == "Publisher")
                {
                    KeyType = SecretKeyType.Publisher;
                }
                else if (keyType == "Subscriber")
                {
                    KeyType = SecretKeyType.Subscriber;
                }
            }
        }


        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Environment"))
                {
                    context.HttpContext.Response.Headers["X-Environment"] = "Development";
                }
            }
            var token = context.HttpContext.Request.Headers["X-Secret-Token"];
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new BadRequestObjectResult("X-Secret-Token not set.");
                return;
            }
            var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
            var result = await mediator.Send(new FindByCurrentToken(token));
            if (result == null)
            {
                context.Result = new UnauthorizedObjectResult("X-Secret-Token is invalid.");
                return;
            }
            await mediator.Publish(new SecretKeyConnected(result.Id, !result.IsConnected));
            await base.OnActionExecutionAsync(context, next);
        }
    }
}