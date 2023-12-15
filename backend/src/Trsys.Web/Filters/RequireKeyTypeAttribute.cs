using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using Trsys.Models;

namespace Trsys.Web.Filters
{
    public class RequireKeyType : ActionFilterAttribute
    {
        public SecretKeyType? KeyType { get; }
        public string KeyTypeStr { get; }

        public RequireKeyType(string keyType = null)
        {
            KeyTypeStr = keyType;
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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                if (!context.HttpContext.Response.Headers.ContainsKey("X-Environment"))
                {
                    context.HttpContext.Response.Headers["X-Environment"] = "Development";
                }
            }
            if (!context.HttpContext.User.HasClaim(claim => claim.Type == ClaimTypes.Role && claim.Value == KeyTypeStr))
            {
                context.Result = new BadRequestObjectResult("InvalidKeyType");
            }
        }
    }
}