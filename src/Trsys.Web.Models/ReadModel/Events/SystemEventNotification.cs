using MediatR;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class SystemEventNotification : INotification
    {
        public SystemEventNotification(string category, string eventType, object data = null)
        {
            Category = category;
            EventType = eventType;
            Data = data;
        }

        public string Category { get; }
        public string EventType { get; }
        public object Data { get; }
    }
}
