using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Infrastructure
{
    public interface ILogDatabase
    {
        Task AddRangeAsync(IEnumerable<LogDto> logs);
        Task<IEnumerable<LogDto>> SearchAsync(string source, int page, int perPage);
    }
}
