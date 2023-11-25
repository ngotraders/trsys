using System.Threading.Tasks;

namespace Trsys.Infrastructure.ReadModel.UserNotification
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string emailAddress, string subject, string body);
    }

    public class EmailSender : IEmailSender
    {
        public EmailSender(EmailSenderConfiguration configuration)
        {
        }

        public Task SendEmailAsync(string emailAddress, string subject, string body)
        {
            return Task.CompletedTask;
        }
    }

    public class EmailSenderConfiguration
    {
        public string Host { get; set; }
    }
}