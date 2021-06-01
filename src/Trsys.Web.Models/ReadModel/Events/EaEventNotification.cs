using MediatR;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class EaEventNotification : INotification
    {
        public EaEventNotification(string key, string eventType, object data = null)
        {
            Key = key;
            EventType = eventType;
            Data = data;
        }

        public string Key { get; }
        public string EventType { get; }
        public object Data { get; }
    }
}
