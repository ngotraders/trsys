using System.Threading.Tasks;

namespace Trsys.Web.Models
{
    public interface IUserRepository
    {
        Task<User> FindByUsernameAsync(string username);
        Task SaveAsync(User user);
    }
}
