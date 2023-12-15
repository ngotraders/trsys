using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Trsys.Web.Filters
{
    public class MinimumEaVersionAttribute : ActionFilterAttribute
    {
        public MinimumEaVersionAttribute(string version)
        {
            Version = version;
        }

        public string Version { get; }

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
            var version = (string)context.HttpContext.Request.Headers["X-Ea-Version"] ?? (string)context.HttpContext.Request.Headers["Version"];
            if (string.IsNullOrEmpty(version))
            {
                context.Result = new BadRequestObjectResult("InvalidVersion");
                return;
            }
            if (version.CompareTo(Version) < 0)
            {
                context.Result = new BadRequestObjectResult("InvalidVersion");
                return;
            }
        }
    }
}
