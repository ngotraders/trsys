using CQRSlite.Events;
using MediatR;
using Newtonsoft.Json;
using SqlStreamStore.Streams;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Events;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public static class StreamMessageConverter
    {

        private static Type[] types = new[]
        {
            typeof(EaEventNotification),
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
            typeof(SystemEventNotification),
            typeof(UserCreated),
            typeof(UserEventNotification),
            typeof(UserPasswordHashChanged),
            typeof(WorldStateCreated),
            typeof(WorldStateSecretKeyDeleted),
            typeof(WorldStateSecretKeyIdGenerated),
            typeof(WorldStateUserDeleted),
            typeof(WorldStateUserIdGenerated),
        };
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
        public static async Task<IEvent> ConvertToEvent(StreamMessage message)
        {
            return (IEvent)JsonConvert.DeserializeObject(await message.GetJsonData(), strToType(message.Type));
        }
        public static async Task<INotification> ConvertToNotification(StreamMessage message)
        {
            return (INotification)JsonConvert.DeserializeObject(await message.GetJsonData(), strToType(message.Type));
        }
        public static NewStreamMessage ConvertFromEvent(IEvent notification)
        {
            return new NewStreamMessage(Guid.NewGuid(), objToType(notification).FullName, JsonConvert.SerializeObject(notification));
        }
    }
}
