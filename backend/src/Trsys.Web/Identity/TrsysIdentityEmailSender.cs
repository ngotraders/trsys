using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Trsys.Web.Identity;

public class TrsysIdentityEmailSender(Infrastructure.ReadModel.UserNotification.IEmailSender emailSender) : IEmailSender<TrsysUser>
{
    public Task SendConfirmationLinkAsync(TrsysUser user, string email, string confirmationLink)
    {
        return emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetCodeAsync(TrsysUser user, string email, string resetCode)
    {
        return emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by entering this code: {resetCode}");
    }

    public Task SendPasswordResetLinkAsync(TrsysUser user, string email, string resetLink)
    {
        return emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
    }
}
