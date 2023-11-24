using Newtonsoft.Json.Linq;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class SqlStreamEventDatabase : IEventDatabase
    {
        private readonly IStreamStore db;

        public SqlStreamEventDatabase(IStreamStore store)
        {
            this.db = store;
        }
        public async Task<IEnumerable<EventDto>> SearchAsync(string source, int page, int perPage)
        {
            if (string.IsNullOrEmpty(source))
            {
                var fromPosition = await db.ReadHeadPosition() - (page - 1) * perPage;
                if (fromPosition < 0)
                {
                    return Array.Empty<EventDto>();
                }
                var messages = await db.ReadAllBackwards(fromPosition, perPage, true);
                return messages.Messages.Select(ConvertToEvent).ToList();
            }
            else
            {
                var fromVersion = (await db.ReadStreamHeadVersion(new StreamId(source)) - (page - 1) * perPage);
                if (fromVersion < 0)
                {
                    return Array.Empty<EventDto>();
                }
                var messages = await db.ReadStreamBackwards(new StreamId(source), fromVersion, perPage, true);
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
                EventType = message.Type.Replace("Trsys.Models.", ""),
                AggregateId = message.StreamId,
                Version = version,
                Data = obj.ToString(),
            };
        }
    }
}
