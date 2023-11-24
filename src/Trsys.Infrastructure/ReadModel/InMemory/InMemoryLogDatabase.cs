using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure.Queue;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public class InMemoryLogDatabase : ILogDatabase, IDisposable
    {
        private readonly BlockingTaskQueue queue = new();
        private readonly List<LogDto> All = new();
        private readonly Dictionary<string, List<LogDto>> BySource = new();

        public Task AddRangeAsync(IEnumerable<LogDto> logs)
        {
            return queue.Enqueue(() =>
            {
                foreach (var log in logs)
                {
                    All.Add(log);
                    if (!BySource.TryGetValue(log.Key, out var list))
                    {
                        list = new();
                        BySource.Add(log.Key, list);
                    }
                    list.Add(log);
                }
            });
        }

        public Task<IEnumerable<LogDto>> SearchAsync(string source, int page, int perPage)
        {
            var events = (string.IsNullOrEmpty(source)
                ? All
                : BySource.TryGetValue(source, out var list)
                ? list
                : new List<LogDto>())
                .AsEnumerable()
                .Reverse();
            if (perPage > 0)
            {
                return Task.FromResult(events.Skip((page - 1) * perPage).Take(perPage));
            }
            return Task.FromResult(events);
        }
        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
