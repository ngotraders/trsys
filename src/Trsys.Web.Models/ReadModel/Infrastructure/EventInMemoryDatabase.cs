using System.Collections.Generic;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Infrastructure
{
    public class EventInMemoryDatabase
    {
        public readonly List<EventDto> All = new();
        public readonly Dictionary<string, EventDto> ById = new();
        public readonly Dictionary<string, List<EventDto>> BySource = new();

        public void Add(EventDto e)
        {
            All.Add(e);
            ById.Add(e.Id, e);
            if (!BySource.TryGetValue(e.Source, out var list))
            {
                list = new();
                BySource.Add(e.Source, list);
            }
            list.Add(e);
        }
    }
}
