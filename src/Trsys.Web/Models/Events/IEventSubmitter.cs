using System.Threading.Tasks;

namespace Trsys.Web.Models.Events
{
    public interface IEventSubmitter
    {
        Task SendAsync(Event ev);
    }
}
