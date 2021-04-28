using System.Threading.Tasks;

namespace Trsys.Web.Models.Users
{
    public interface IUserRepository
    {
        Task<User> FindByUsernameAsync(string username);
        Task SaveAsync(User user);
    }
}
