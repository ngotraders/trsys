using Microsoft.AspNetCore.Builder;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Middlewares;

public static class InitializationMiddlewareExtension
{
    public static IApplicationBuilder UseInitialization(this IApplicationBuilder app)
    {
        var task = Task.Run(async () =>
        {
            await DatabaseInitializer.InitializeAsync(app);
            await DatabaseInitializer.SeedDataAsync(app);
        });

        // 最大で1秒待つ
        Task.WhenAny(Task.Delay(1000), task).Wait();
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
