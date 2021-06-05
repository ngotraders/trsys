using System.Collections.Generic;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Infrastructure
{
    public class LogInMemoryDatabase
    {
        public readonly List<LogDto> All = new();
        public readonly Dictionary<string, List<LogDto>> BySource = new();

        public void Add(LogDto e)
        {
            All.Add(e);
            if (!BySource.TryGetValue(e.Key, out var list))
            {
                list = new();
                BySource.Add(e.Key, list);
            }
            list.Add(e);
        }
    }
}
