using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Trsys.Infrastructure.ReadModel.InMemory;
using Trsys.Infrastructure.ReadModel.UserNotification;
using Trsys.Models;

namespace Trsys.Infrastructure.Tests
{
    [TestClass]
    [Ignore]
    public class MailKitEmailSenderTests
    {
        [TestMethod]
        public async Task When_sending_email_then_mail_sent()
        {
            var configurationDb = new InMemoryConfigurationDatabase();
            await configurationDb.SaveEmailConfigurationAsync(new EmailConfiguration
            {
                Host = "localhost",
                Port = 1025,
                UseSsl = false,
                MailFrom = "copy-trading-system@example.com",
            });

            var sut = new MailKitEmailSender(configurationDb);
            await sut.SendEmailAsync("copy-trading-system@example.com", "subject", "body");
        }
    }
}
