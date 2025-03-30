using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Infrastructure.Queue;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.UserNotification;

public class EmailMessageUserNotificationDispatcher : BackgroundService, IUserNotificationDispatcher
{
    private readonly IEmailSender emailSender;
    private readonly IUserDatabase userDatabase;
    private readonly ILogger logger;
    private readonly BlockingTaskQueue queue = new();

    public EmailMessageUserNotificationDispatcher(IEmailSender emailSender, IUserDatabase userDatabase, ILogger<EmailMessageUserNotificationDispatcher> logger)
    {
        this.emailSender = emailSender;
        this.userDatabase = userDatabase;
        this.logger = logger;
    }

    public void DispatchSystemNotification(NotificationMessageDto message)
    {
        queue.Enqueue(async () =>
        {
            var users = await userDatabase.SearchAsync();
            await emailSender.SendEmailsAsync(users.Select(user => user.EmailAddress).Where(a => !string.IsNullOrEmpty(a)).ToList(), message.Subject, message.Body);
        }).ContinueWith(e =>
        {
            if (e.Exception != null)
            {
                logger.LogError(e.Exception, "メール送信でエラーが発生しました。");
            }
            else
            {
                logger.LogInformation("メールが送信されました。");
            }
        });
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tcs = new TaskCompletionSource();
        stoppingToken.Register(() =>
        {
            this.queue.Dispose();
            tcs.SetResult();
        });
        return tcs.Task;
    }
}