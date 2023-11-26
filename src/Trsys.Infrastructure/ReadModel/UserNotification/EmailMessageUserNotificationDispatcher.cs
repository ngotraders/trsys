using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.UserNotification
{
    public class EmailMessageUserNotificationDispatcher : IUserNotificationDispatcher
    {
        private readonly IEmailSender emailSender;
        private readonly IUserDatabase userDatabase;

        public EmailMessageUserNotificationDispatcher(IEmailSender emailSender, IUserDatabase userDatabase)
        {
            this.emailSender = emailSender;
            this.userDatabase = userDatabase;
        }

        public async Task DispatchSystemNotificationAsync(NotificationMessageDto message)
        {
            var users = await userDatabase.SearchAsync();
            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.EmailAddress))
                {
                    continue;
                }
                await emailSender.SendEmailAsync(user.EmailAddress, message.Subject, message.Body);
            }
        }
    }
}