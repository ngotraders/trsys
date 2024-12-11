using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Identity.Client;
using MimeKit;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.UserNotification;

public class MailKitEmailSender : IEmailSender
{
    private readonly IConfigurationDatabase configurationDb;

    public MailKitEmailSender(IConfigurationDatabase configurationDb)
    {
        this.configurationDb = configurationDb;
    }

    public async Task SendEmailAsync(string emailAddress, string subject, string body)
    {
        var configuration = await configurationDb.GetEmailConfigurationAsync();
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
            client.Connect(configuration.Host, configuration.Port ?? 25, (configuration.UseSsl ?? false) ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None);

            if (!string.IsNullOrEmpty(configuration.AuthenticationUser))
            {
                if (string.IsNullOrEmpty(configuration.AuthenticationAuthority))
                {
                    client.Authenticate(configuration.AuthenticationUser, configuration.AuthenticationPassword);
                }
                else
                {
                    var confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(configuration.AuthenticationClientId)
                        .WithAuthority(configuration.AuthenticationAuthority)
                        .WithClientSecret(configuration.AuthenticationClientSecret)
                        .Build();

                    var authToken = await confidentialClientApplication.AcquireTokenForClient(configuration.AuthenticationScopes).ExecuteAsync();
                    var oauth2 = new SaslMechanismOAuth2(configuration.AuthenticationUser, authToken.AccessToken);
                    client.Authenticate(oauth2);
                }
            }

            await client.SendAsync(message);
            client.Disconnect(true);
        }
    }
}