using Azure.Identity;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.UserNotification;

public class EmailSender : IEmailSender
{
    private readonly IConfigurationDatabase configurationDb;

    public EmailSender(IConfigurationDatabase configurationDb)
    {
        this.configurationDb = configurationDb;
    }

    public Task SendEmailAsync(string emailAddress, string subject, string body)
    {
        return SendEmailsAsync([emailAddress], subject, body);
    }

    public async Task SendEmailsAsync(List<string> emailAddresses, string subject, string body)
    {
        var configuration = await configurationDb.GetEmailConfigurationAsync();
        if (string.IsNullOrEmpty(configuration.MailFrom))
        {
            return;
        }

        if (configuration.Smtp != null && configuration.Smtp.IsValid)
        {
            var smtpConfig = configuration.Smtp;
            using var client = new SmtpClient();
            client.Connect(smtpConfig.Host, smtpConfig.Port ?? 25, (smtpConfig.UseSsl ?? false) ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None);

            if (!string.IsNullOrEmpty(smtpConfig.AuthenticationUser))
            {
                client.Authenticate(smtpConfig.AuthenticationUser, smtpConfig.AuthenticationPassword);
            }

            foreach (var emailAddress in emailAddresses)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(configuration.MailFrom, configuration.MailFrom));
                message.To.Add(new MailboxAddress(emailAddress, emailAddress));
                message.Subject = subject;

                message.Body = new TextPart("plain")
                {
                    Text = body
                };
                await client.SendAsync(message);
            }

            client.Disconnect(true);
        }
        if (configuration.Graph != null && configuration.Graph.IsValid)
        {
            var graphConfig = configuration.Graph;
            var clientSecretCredential = new ClientSecretCredential(
                graphConfig.TenantId,
                graphConfig.ClientId,
                graphConfig.ClientSecret,
                new ClientSecretCredentialOptions()
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                });

            using var graphClient = new GraphServiceClient(clientSecretCredential);

            foreach (var emailAddress in emailAddresses)
            {
                var requestBody = new SendMailPostRequestBody()
                {
                    Message = new Message
                    {
                        Subject = subject,
                        Body = new ItemBody
                        {
                            ContentType = BodyType.Text,
                            Content = body
                        },
                        ToRecipients = [
                            new Recipient
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Address = emailAddress
                                }
                            }
                        ]
                    },
                    SaveToSentItems = true,
                };

                await graphClient.Users[configuration.MailFrom].SendMail.PostAsync(requestBody);
            }
        }
    }
}