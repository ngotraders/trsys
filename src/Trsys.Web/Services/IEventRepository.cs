using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Services
{
    public interface IEventRepository
    {
        Task<List<Event>> SearchAllAsync();
        Task<List<Event>> SearchAsync(string source, int page, int perPage);
        Task SaveAsync(Event ev);
    }
}
