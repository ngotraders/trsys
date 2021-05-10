using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trsys.Web.Models.Events
{
    public interface IEventRepository
    {
        Task<List<Event>> SearchAllAsync();
        Task<List<Event>> SearchAsync(string key, int page, int perPage);
        Task SaveAsync(Event ev);
    }
}
