using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Models.Events;

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

        public Task SaveAsync(Event ev)
        {
            db.Events.Add(ev);
            return db.SaveChangesAsync();
        }
    }
}
