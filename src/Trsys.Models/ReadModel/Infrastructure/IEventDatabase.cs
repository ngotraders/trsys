using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface IEventDatabase
    {
        Task<IEnumerable<EventDto>> SearchAsync(string source, int page, int perPage);
    }
}
