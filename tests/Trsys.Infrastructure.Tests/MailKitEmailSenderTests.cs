using System.Collections.Generic;
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
                Host = "localhost",
                Port = 1025,
                UseSsl = false,
                MailFrom = "copy-trading-system@example.com",
            };

            var sut = new MailKitEmailSender(configuration);
            await sut.SendEmailAsync("copy-trading-system@example.com", "subject", "body");
        }
    }
}
