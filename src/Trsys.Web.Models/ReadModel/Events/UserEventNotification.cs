using MediatR;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class UserEventNotification: INotification
    {
        public UserEventNotification(string username, string eventType, object data = null)
        {
            Username = username;
            EventType = eventType;
            Data = data;
        }

        public string Username { get; }
        public string EventType { get; }
        public object Data { get; }
    }
}
