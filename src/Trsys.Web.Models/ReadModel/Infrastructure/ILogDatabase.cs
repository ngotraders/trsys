using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Infrastructure
{
    public interface ILogDatabase
    {
        Task AddRangeAsync(IEnumerable<LogDto> logs);
        Task<IEnumerable<LogDto>> SearchAsync(string source, int page, int perPage);
    }
}
