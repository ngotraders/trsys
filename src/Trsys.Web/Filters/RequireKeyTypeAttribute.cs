using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Trsys.Web.Models;

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
            if (!context.HttpContext.User.HasClaim(claim => claim.Type == ClaimTypes.Role && claim.Value == KeyTypeStr))
            {
                context.Result = new BadRequestObjectResult("InvalidKeyType");
            }
        }
    }
}