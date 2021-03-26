using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Filters
{
    public class SecretTokenFilter : TypeFilterAttribute
    {
        public SecretTokenFilter() : base(typeof(SecretTokenFilterImpl))
        {
        }

        private class SecretTokenFilterImpl : IAsyncActionFilter
        {
            private readonly ITokenValidator validator;

            public SecretTokenFilterImpl(ITokenValidator validator)
            {
                this.validator = validator;
            }

            public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var token = context.HttpContext.Request.Headers["X-Secret-Token"];
                if (!validator.Validate(token))
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return Task.CompletedTask;
                }
                return next();
            }
        }
    }
}