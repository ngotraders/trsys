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

        public async Task<int> CountAsync(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return (int)await db.ReadHeadPosition();
            }
            else
            {
                return (int)await db.ReadStreamHeadVersion(new StreamId(source));
            }
        }

        public async Task<List<EventDto>> SearchAsync()
        {
            var messages = await db.ReadAllBackwards(0, int.MaxValue, true);
            return messages.Messages.Select(ConvertToEventDto).ToList();
        }

        public async Task<List<EventDto>> SearchAsync(int start, int end, string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                var fetchCount = end - start;
                var headPosition = await db.ReadHeadPosition();
                if (headPosition < start)
                {
                    return [];
                }
                var messages = await db.ReadAllBackwards(Math.Min(headPosition, end), fetchCount, true);
                return messages.Messages.Select(ConvertToEventDto).ToList();
            }
            else
            {
                var streamId = new StreamId(source);
                var fetchCount = end - start;
                var headVersion = await db.ReadStreamHeadVersion(streamId);
                if (headVersion < start)
                {
                    return [];
                }
                var messages = await db.ReadStreamBackwards(streamId, Math.Min(headVersion, end), fetchCount, true);
                return messages.Messages.Select(ConvertToEventDto).ToList();
            }
        }

        private static EventDto ConvertToEventDto(StreamMessage message)
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
