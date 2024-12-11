using System.Threading.Tasks;

namespace Trsys.Infrastructure.ReadModel.UserNotification;

public interface IEmailSender
{
    Task SendEmailAsync(string emailAddress, string subject, string body);
}