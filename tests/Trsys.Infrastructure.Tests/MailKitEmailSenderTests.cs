using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trsys.Infrastructure.ReadModel.UserNotification;

namespace Trsys.Infrastructure.Tests
{
    [TestClass]
    [Ignore]
    public class MailKitEmailSenderTests
    {
        [TestMethod]
        public async Task When_sending_email_then_mail_sent()
        {
            var configuration = new EmailSenderConfiguration
            {
                Host = "smtp",
                Port = 1025,
                UseSsl = false,
                // AuthenticationUser = "user",
                // AuthenticationPassword = "password",
                MailFrom = "test@smtp",
            };
            var sut = new MailKitEmailSender(configuration);
            await sut.SendEmailAsync("target@smtp", "subject", "body");
        }
    }
}