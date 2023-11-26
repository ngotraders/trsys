using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface IUserNotificationDispatcher
    {
        Task DispatchSystemNotificationAsync(NotificationMessageDto message);
    }
}