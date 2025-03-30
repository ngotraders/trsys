using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trsys.Infrastructure.ReadModel.UserNotification;

public class DummyEmailSender : IEmailSender
{
    public Task SendEmailAsync(string emailAddress, string subject, string body)
    {
        return Task.CompletedTask;
    }
    public Task SendEmailsAsync(List<string> emailAddresses, string subject, string body)
    {
        return Task.CompletedTask;
    }
}