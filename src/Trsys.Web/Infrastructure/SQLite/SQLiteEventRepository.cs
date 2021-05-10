using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Generic;
using Trsys.Web.Models.Events;

namespace Trsys.Web.Infrastructure.SQLite
{
    public class SQLiteEventRepository : IEventRepository
    {
        private readonly TrsysContextProcessor processor;

        public SQLiteEventRepository(TrsysContextProcessor processor)
        {
            this.processor = processor;
        }

        public Task<List<Event>> SearchAllAsync()
        {
            return processor.Enqueue(db => new EventRepository(db).SearchAllAsync());
        }

        public Task<List<Event>> SearchAsync(string key, int page, int perPage)
        {
            return processor.Enqueue(db => new EventRepository(db).SearchAsync(key, page, perPage));
        }

        public Task SaveAsync(Event ev)
        {
            return processor.Enqueue(db => new EventRepository(db).SaveAsync(ev));
        }
    }
}
