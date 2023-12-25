using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface IEventDatabase
    {
        Task<int> CountAsync(string source);
        Task<List<EventDto>> SearchAsync();
        Task<List<EventDto>> SearchAsync(int start, int end, string source);
    }
}
