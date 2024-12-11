using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.UserNotification;

public class EmailMessageUserNotificationDispatcher : BackgroundService, IUserNotificationDispatcher
{
    private readonly IEmailSender emailSender;
    private readonly IUserDatabase userDatabase;
    private readonly BlockingCollection<NotificationMessageDto> queue = [];

    public EmailMessageUserNotificationDispatcher(IEmailSender emailSender, IUserDatabase userDatabase)
    {
        this.emailSender = emailSender;
        this.userDatabase = userDatabase;
    }

    public void DispatchSystemNotification(NotificationMessageDto message)
    {
        queue.Add(message);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var message = queue.Take(stoppingToken);
                    var users = await userDatabase.SearchAsync();
                    foreach (var user in users)
                    {
                        if (string.IsNullOrEmpty(user.EmailAddress))
                        {
                            continue;
                        }
                        await emailSender.SendEmailAsync(user.EmailAddress, message.Subject, message.Body);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        });
    }
}