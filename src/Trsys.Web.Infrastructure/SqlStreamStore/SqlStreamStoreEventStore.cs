using CQRSlite.Events;
using MediatR;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.SqlStreamStore
{
    public class SqlStreamStoreEventStore : IEventStore
    {
        private readonly IMediator mediator;
        private readonly IStreamStore store;

        public SqlStreamStoreEventStore(IMediator mediator, IStreamStore store)
        {
            this.mediator = mediator;
            this.store = store;
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            var lookups = events.ToLookup(e => e.Id);
            var messages = new List<PublishingMessage>();
            foreach (var lookup in lookups)
            {
                var e = lookup.OrderBy(e => e.Version).ToList();
                var first = e.First();
                var newMessages = e.Select(i => MessageConverter.ConvertFromEvent(i)).ToArray();
                messages.AddRange(newMessages);
                var result = await store.AppendToStream(
                    new StreamId(first.Id.ToString()),
                    first.Version == 1 ? ExpectedVersion.NoStream : first.Version - 2,
                    newMessages.Select(m => new NewStreamMessage(m.Id, m.Type, m.Data)).ToArray(),
                    cancellationToken);
            }
            await mediator.Publish(new PublishingMessageEnvelope(messages), cancellationToken);
        }

        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
        {
            var events = new List<IEvent>();
            var messages = await store.ReadStreamForwards(new StreamId(aggregateId.ToString()), fromVersion < 0 ? 0 : fromVersion, int.MaxValue, cancellationToken);
            foreach (var message in messages.Messages)
            {
                events.Add(MessageConverter.ConvertToEvent(new PublishingMessage()
                {
                    Id = message.MessageId,
                    Type = message.Type,
                    Data = await message.GetJsonData()
                }));
            }
            return events;
        }
    }
}
