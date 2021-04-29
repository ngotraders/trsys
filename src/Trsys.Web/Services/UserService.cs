using System.Threading.Tasks;
using Trsys.Web.Models.Users;

namespace Trsys.Web.Services
{
    public class UserService
    {
        private readonly IUserRepository repository;

        public UserService(IUserRepository repository)
        {
            this.repository = repository;
        }

        public Task<User> FindByUsernameAsync(string username)
        {
            return repository.FindByUsernameAsync(username);
        }

        public async Task ChangePasswordAsync(string username, string password)
        {
            var user = await repository.FindByUsernameAsync(username);
            user.Password = password;
            await repository.SaveAsync(user);
        }
    }
}
