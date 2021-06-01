using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Tokens;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Filters
{
    public static class SecretKeyClaimsPrincipalFactory
    {
        private static readonly Tuple<string, SecretKeyType>[] secretKeyTypes = Enum.GetValues(typeof(SecretKeyType))
            .OfType<SecretKeyType>()
            .Select(key => Tuple.Create(Enum.GetName(typeof(SecretKeyType), key), key))
            .ToArray();

        public static ClaimsPrincipal Create(Guid id, string key, SecretKeyType keyType)
        {
            var principal = new ClaimsPrincipal();
            var claims = new List<Claim>() {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, key)
            };
            foreach (var elem in secretKeyTypes)
            {
                if (keyType.HasFlag(elem.Item2))
                {
                    claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, elem.Item1));
                }
            }
            principal.AddIdentity(new ClaimsIdentity(claims, "SecretToken", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType));
            return principal;
        }
    }

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
                context.Result = new UnauthorizedObjectResult("X-Secret-Token not se.");
                return;
            }
            var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
            var result = await mediator.Send(new FindByCurrentToken(token));
            if (result == null)
            {
                context.Result = new UnauthorizedObjectResult("X-Secret-Token is invalid.");
                return;
            }
            await mediator.Publish(new TokenTouched(token));
            context.HttpContext.User = SecretKeyClaimsPrincipalFactory.Create(result.Id, result.Key, result.KeyType.Value);
            await base.OnActionExecutionAsync(context, next);
        }
    }
}