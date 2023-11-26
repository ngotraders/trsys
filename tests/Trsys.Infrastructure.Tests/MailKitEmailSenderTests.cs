using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trsys.Infrastructure.ReadModel.UserNotification;

namespace Trsys.Infrastructure.Tests
{
    [TestClass]
    public class MailKitEmailSenderTests
    {
        [TestMethod]
        public async Task When_sending_email_then_mail_sent()
        {
            var configuration = new EmailSenderConfiguration
            {
                Host = "smtp.office365.com",
                Port = 587,
                UseSsl = true,
                // AuthenticationUser = "copy-trading-system@xsys.co.jp",
                // AuthenticationPassword = "Xuv15129",
                // AuthenticationUser = "yuto.nagano@xsys.co.jp",
                // AuthenticationPassword = "Gs9q4if9",
                AuthenticationClientId = "3dca753e-90d3-4840-915f-a7521490e7cf",
                AuthenticationAuthority = "https://login.microsoftonline.com/748076ca-4ade-484a-8c5d-7186653aad71/v2.0",
                AuthenticationClientSecret = "ADG8Q~K5T1p4jjG8o2aPR9yX4WqbthhcxryUMcVg",
                AuthenticationScopes = new List<string> { "https://outlook.office365.com/.default" },
                AuthenticationUser = "copy-trading-system@xsys.co.jp",
                MailFrom = "copy-trading-system@xsys.co.jp",
            };

            var sut = new MailKitEmailSender(configuration);
            await sut.SendEmailAsync("yuto.nagano@xsys.co.jp", "subject", "body");
        }
    }
}
