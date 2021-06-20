using Microsoft.AspNetCore.Builder;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Trsys.Web.Middlewares
{
    public static class InitializationMiddlewareExtension
    {
        public static IApplicationBuilder UseInitialization(this IApplicationBuilder app, Task task)
        {
            var textBytes = Encoding.UTF8.GetBytes("Initializing Services");
            var waiter = Task.Delay(100);
            return app.Use(async (context, next) =>
            {
                await Task.WhenAny(task, waiter);
                if (!task.IsCompleted)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    await context.Response.Body.WriteAsync(textBytes);
                    return;
                }
                await next();
            });
        }
    }
}
