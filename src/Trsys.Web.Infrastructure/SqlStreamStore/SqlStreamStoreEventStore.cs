using CQRSlite.Events;
using MediatR;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Events;

namespace Trsys.Web.Infrastructure.InMemory
{
    public class SqlStreamStoreEventStore : IEventStore
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

        private readonly IMediator mediator;
        private readonly IStreamStore store;

        public SqlStreamStoreEventStore(IMediator mediator, IStreamStore store)
        {
            this.mediator = mediator;
            this.store = store;
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            foreach (var lookup in events.ToLookup(e => e.Id))
            {
                var e = lookup.OrderBy(e => e.Version).ToList();
                var first = e.First();
                await store.AppendToStream(
                    new StreamId(first.Id.ToString()),
                    first.Version == 1 ? ExpectedVersion.NoStream : first.Version - 2,
                    e.Select(i => new NewStreamMessage(Guid.NewGuid(), objToType(i).FullName, JsonConvert.SerializeObject(i))).ToArray(),
                    cancellationToken);
            }
            foreach (var @event in events)
            {
                await mediator.Publish(@event, cancellationToken);
            }
        }

        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
        {
            var events = new List<IEvent>();
            var messages = await store.ReadStreamForwards(new StreamId(aggregateId.ToString()), fromVersion < 0 ? 0 : fromVersion, int.MaxValue, cancellationToken);
            foreach (var message in messages.Messages)
            {
                var @event = (IEvent)JsonConvert.DeserializeObject(await message.GetJsonData(), strToType(message.Type));
                events.Add(@event);
            }
            return events;
        }
    }
}
