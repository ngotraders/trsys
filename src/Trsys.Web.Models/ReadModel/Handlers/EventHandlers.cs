using MediatR;
using Newtonsoft.Json.Linq;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Models.ReadModel.Handlers
{
    public class EventHandlers : IRequestHandler<GetEvents, IEnumerable<EventDto>>
    {
        private readonly IStreamStore store;

        public EventHandlers(IStreamStore store)
        {
            this.store = store;
        }

        public async Task<IEnumerable<EventDto>> Handle(GetEvents request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Source))
            {
                var fromPosition = await store.ReadHeadPosition() - (request.Page - 1) * request.PerPage;
                if (fromPosition < 0)
                {
                    return Array.Empty<EventDto>();
                }
                var messages = await store.ReadAllBackwards(fromPosition, request.PerPage, true, cancellationToken);
                return messages.Messages.Select(ConvertToEvent).ToList();
            }
            else
            {
                var fromVersion = (await store.ReadStreamHeadVersion(new StreamId(request.Source)) - (request.Page - 1) * request.PerPage);
                if (fromVersion < 0)
                {
                    return Array.Empty<EventDto>();
                }
                var messages = await store.ReadStreamBackwards(new StreamId(request.Source), fromVersion, request.PerPage, true, cancellationToken);
                return messages.Messages.Select(ConvertToEvent).ToList();
            }
        }

        private static EventDto ConvertToEvent(StreamMessage message)
        {
            var obj = JObject.Parse(message.GetJsonData().Result);
            var timestamp = obj.Property("TimeStamp").Value.Value<DateTime>();
            var version = int.Parse(obj.Property("Version").Value.ToString());
            obj.Remove("Id");
            obj.Remove("Version");
            obj.Remove("TimeStamp");
            return new EventDto()
            {
                Id = message.MessageId.ToString(),
                Timestamp = timestamp,
                EventType = message.Type.Replace("Trsys.Web.Models.ReadModel.Events.", ""),
                AggregateId = message.StreamId,
                Version = version,
                Data = obj.ToString(),
            };
        }
    }
}
