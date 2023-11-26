using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Trsys.Infrastructure.ReadModel.UserNotification
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string emailAddress, string subject, string body);
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailSenderConfiguration configuration;

        public EmailSender(EmailSenderConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(string emailAddress, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(configuration.MailFrom, configuration.MailFrom));
            message.To.Add(new MailboxAddress(emailAddress, emailAddress));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                client.Connect(configuration.Host, configuration.Port, configuration.UseSsl);

                if (!string.IsNullOrEmpty(configuration.AuthenticationUser))
                {
                    client.Authenticate(configuration.AuthenticationUser, configuration.AuthenticationPassword);
                }

                await client.SendAsync(message);
                client.Disconnect(true);
            }
        }
    }

    public class EmailSenderConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string AuthenticationUser { get; set; }
        public string AuthenticationPassword { get; set; }
        public string MailFrom { get; set; }
    }
}