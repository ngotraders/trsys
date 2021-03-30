using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Trsys.Web.Filters
{
    public class EaVersionAttribute : ActionFilterAttribute
    {
        public EaVersionAttribute(string version)
        {
            Version = version;
        }

        public string Version { get; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Headers["Version"] != Version)
            {
                context.Result = new BadRequestObjectResult("InvalidVersion");
            }
        }

    }
}
