using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Models;

namespace Trsys.Web.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly TrsysContext db;

        public UserRepository(TrsysContext db)
        {
            this.db = db;
        }

        public Task<User> FindByUsernameAsync(string username)
        {
            return db.Users.FirstOrDefaultAsync(e => e.Username == username);
        }

        public Task SaveAsync(User entity)
        {
            if (entity.Id > 0)
            {
                db.Users.Update(entity);
            }
            else
            {
                db.Users.Add(entity);
            }
            return db.SaveChangesAsync();
        }
    }
}
