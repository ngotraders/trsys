using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Filters
{
    public class EaEndpointAttribute : ActionFilterAttribute
    {
        private static readonly Dictionary<string, SecretKeyType> secretKeyTypes = Enum.GetValues(typeof(SecretKeyType))
            .OfType<SecretKeyType>()
            .ToDictionary(key => Enum.GetName(typeof(SecretKeyType), key), key => key);

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                context.HttpContext.Response.Headers.Add("X-Environment", "Development");
            }
            var key = (string)context.HttpContext.Request.Headers["X-Ea-Id"];
            var type = (string)context.HttpContext.Request.Headers["X-Ea-Type"];
            var version = (string)context.HttpContext.Request.Headers["X-Ea-Version"];
            if (string.IsNullOrEmpty(key))
            {
                context.Result = new BadRequestObjectResult("X-Ea-Id is not set.");
                return;
            }
            if (string.IsNullOrEmpty(type))
            {
                context.Result = new BadRequestObjectResult("X-Ea-Type is not set.");
                return;
            }
            if (string.IsNullOrEmpty(version))
            {
                context.Result = new BadRequestObjectResult("X-Ea-Version is not set.");
                return;
            }

            if (context.HttpContext.Request.Path.StartsWithSegments("/api/ea/logs"))
            {
                await base.OnActionExecutionAsync(context, next);
                return;
            }

            var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
            var secretKey = await mediator.Send(new FindBySecretKey(key));
            if (secretKey == null)
            {
                await mediator.Send(new CreateSecretKeyIfNotExistsCommand(secretKeyTypes.TryGetValue(type, out var keyType) ? keyType : null, key, null));
                context.Result = new BadRequestObjectResult("InvalidSecretKey");
                return;
            }
            else if (!secretKey.IsApproved)
            {
                context.Result = new BadRequestObjectResult("InvalidSecretKey");
                return;
            }
            context.HttpContext.User = SecretKeyClaimsPrincipalFactory.Create(secretKey.Id, secretKey.Key, secretKey.KeyType.Value);
            await base.OnActionExecutionAsync(context, next);
        }

    }
}
