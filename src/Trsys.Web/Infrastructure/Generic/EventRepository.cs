using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Models;
using Trsys.Web.Services;

namespace Trsys.Web.Infrastructure.Generic
{
    public class EventRepository : IEventRepository
    {
        private readonly TrsysContext db;

        public EventRepository(TrsysContext db)
        {
            this.db = db;
        }

        public Task<List<Event>> SearchAllAsync()
        {
            return db.Events.ToListAsync();
        }

        public Task<List<Event>> SearchAsync(string source, int page, int perPage)
        {
            var events = db.Events as IQueryable<Event>;
            if (!string.IsNullOrEmpty(source))
            {
                events = events.Where(e => e.Source == source);
            }
            return events.OrderByDescending(e => e.Timestamp).Skip((page - 1) * perPage).Take(perPage).ToListAsync();
        }

        public Task SaveAsync(Event ev)
        {
            db.Events.Add(ev);
            return db.SaveChangesAsync();
        }
    }
}
