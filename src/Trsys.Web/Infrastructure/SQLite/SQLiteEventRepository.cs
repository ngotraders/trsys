using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            return processor.Enqueue(db => db.Events.ToListAsync());
        }

        public Task SaveAsync(Event ev)
        {
            return processor.Enqueue(db =>
            {
                db.Add(ev);
                return db.SaveChangesAsync();
            });
        }
    }
}
