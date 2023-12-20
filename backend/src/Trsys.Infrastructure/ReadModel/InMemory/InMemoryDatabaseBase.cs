using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure.Queue;

namespace Trsys.Infrastructure.ReadModel.InMemory
{
    public abstract class InMemoryDatabaseBase<TItem, TId> : IDisposable
    {
        protected readonly BlockingTaskQueue queue = new();

        protected readonly List<TItem> All = [];
        protected readonly Dictionary<TId, TItem> ById = [];

        public Task AddAsync(TId id, TItem item, Action<TItem> onAdded = null)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryAdd(id, item))
                {
                    All.Add(item);
                    ById[id] = item;
                    if (onAdded != null)
                    {
                        onAdded(item);
                    }
                }
            });
        }

        public Task<TItem> UpdateAsync(TId id, Action<TItem> onUpdate = null)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
                {
                    if (onUpdate != null)
                    {
                        onUpdate(item);
                    }
                    return item;
                }
                throw new InvalidOperationException();
            });
        }

        public Task RemoveAsync(TId id, Action<TItem> onRemove = null)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
                {
                    ById.Remove(id);
                    All.Remove(item);
                    if (onRemove != null)
                    {
                        onRemove(item);
                    }
                }
            });
        }

        public Task<TItem> FindByIdAsync(TId id)
        {
            return queue.Enqueue(() =>
            {
                if (ById.TryGetValue(id, out var item))
                {
                    return item;
                }
                return default;
            });
        }

        public Task<int> CountAsync()
        {
            return Task.FromResult(All.Count);
        }

        public Task<List<TItem>> SearchAsync()
        {
            return Task.FromResult(All.ToList());
        }

        public Task<List<TItem>> SearchAsync(int start, int end, string[] sort, string[] order)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (end <= start)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }
            var query = null as IOrderedEnumerable<TItem>;
            if (sort != null && order != null)
            {
                for (var i = 0; i < sort.Length; i++)
                {
                    var sortKey = sort[i];
                    var orderKey = order[i];
                    if (orderKey == "asc")
                    {
                        if (query == null)
                        {
                            query = All.OrderBy(item => GetItemValue(item, sortKey));
                        }
                        else
                        {
                            query = query.ThenBy(item => GetItemValue(item, sortKey));
                        }
                    }
                    else if (orderKey == "desc")
                    {
                        if (query == null)
                        {
                            query = All.OrderByDescending(item => GetItemValue(item, sortKey));
                        }
                        else
                        {
                            query = query.ThenByDescending(item => GetItemValue(item, sortKey));
                        }
                    }
                }
            }
            return Task.FromResult((query as IEnumerable<TItem> ?? All).Skip(start).Take(end - start).ToList());
        }

        protected abstract object GetItemValue(TItem item, string sortKey);

        public void Dispose()
        {
            queue.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
