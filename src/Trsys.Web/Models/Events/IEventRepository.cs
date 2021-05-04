using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trsys.Web.Models.Events
{
    public interface IEventRepository
    {
        Task<List<Event>> SearchAllAsync();
        Task SaveAsync(Event ev);
    }
}
