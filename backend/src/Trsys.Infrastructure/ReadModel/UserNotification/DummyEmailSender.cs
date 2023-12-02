using System.Threading.Tasks;

namespace Trsys.Infrastructure.ReadModel.UserNotification
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string emailAddress, string subject, string body)
        {
            return Task.CompletedTask;
        }
    }
}