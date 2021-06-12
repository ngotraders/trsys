using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.Messaging;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Notifications;

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
            await mediator.Publish(PublishingMessageEnvelope.Create(new TokenTouched(token)));
            await base.OnActionExecutionAsync(context, next);
        }
    }
}