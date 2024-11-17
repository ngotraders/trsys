using CQRSlite.Events;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Linq.Expressions;
using Trsys.Models.Events;
using Trsys.Models.ReadModel.Notifications;
using Trsys.Models.WriteModel.Notifications;

namespace Trsys.Models.Messaging
{
    public static class MessageConverter
    {

        private static Type[] types = [
            typeof(LogNotification),
            typeof(OrderPublisherClosedOrder),
            typeof(OrderPublisherOpenedOrder),
            typeof(OrderPublisherRegistered),
            typeof(OrderSubscriberClosedOrder),
            typeof(OrderSubscriberOpenedOrder),
            typeof(OrderSubscriberRegistered),
            typeof(SecretKeyApproved),
            typeof(SecretKeyCreated),
            typeof(SecretKeyDeleted),
            typeof(SecretKeyDescriptionChanged),
            typeof(SecretKeyEaConnected),
            typeof(SecretKeyEaDisconnected),
            typeof(SecretKeyKeyTypeChanged),
            typeof(SecretKeyRevoked),
            typeof(SecretKeyTokenGenerated),
            typeof(SecretKeyTokenInvalidated),
            typeof(SecretKeyConnected),
            typeof(UserCreated),
            typeof(UserUserInfoUpdated),
            typeof(UserPasswordHashChanged),
            typeof(WorldStateCreated),
            typeof(WorldStateSecretKeyDeleted),
            typeof(WorldStateSecretKeyIdGenerated),
            typeof(WorldStateUserDeleted),
            typeof(WorldStateUserIdGenerated),
        ];
        private static Func<object, Type>[] objToTypes = types.Select(e =>
        {
            var p = Expression.Parameter(typeof(object));
            var rt = Expression.Label(typeof(Type));
            var rl = Expression.Label(rt, Expression.Default(typeof(Type)));
            return Expression.Lambda(
                Expression.Block(
                Expression.IfThenElse(
                    Expression.TypeIs(p, e),
                    Expression.Return(rt, Expression.Constant(e, typeof(Type))),
                    Expression.Return(rt, Expression.Constant(null, typeof(Type)))
                    ),
                rl),
                p).Compile() as Func<object, Type>;
        }).ToArray();
        private static Func<object, Type> objToType = o => objToTypes.Select(ott => ott(o)).First(t => t != null);
        private static Func<string, Type> strToType = str => types.First(t => t.FullName == str);
        public static IEvent ConvertToEvent(PublishingMessage message)
        {
            return (IEvent)JsonConvert.DeserializeObject(message.Data, strToType(message.Type));
        }
        public static PublishingMessage ConvertFromEvent(IEvent @event)
        {
            return new PublishingMessage()
            {
                Id = Guid.NewGuid(),
                Type = objToType(@event).FullName,
                Data = JsonConvert.SerializeObject(@event)
            };
        }
        public static INotification ConvertToNotification(PublishingMessage message)
        {
            return (INotification)JsonConvert.DeserializeObject(message.Data, strToType(message.Type));
        }
        public static PublishingMessage ConvertFromNotification(INotification notification)
        {
            return new PublishingMessage()
            {
                Id = Guid.NewGuid(),
                Type = objToType(notification).FullName,
                Data = JsonConvert.SerializeObject(notification)
            };
        }
    }
}
