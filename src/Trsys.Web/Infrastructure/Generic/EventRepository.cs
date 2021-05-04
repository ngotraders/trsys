using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            db.Add(ev);
            return db.SaveChangesAsync();
        }
    }
}
