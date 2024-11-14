using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface IUserNotificationDispatcher
    {
        void DispatchSystemNotification(NotificationMessageDto message);
    }
}