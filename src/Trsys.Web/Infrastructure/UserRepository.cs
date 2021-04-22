using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly TrsysContextProcessor processor;

        public UserRepository(TrsysContextProcessor processor)
        {
            this.processor = processor;
        }

        public Task<User> FindByUsernameAsync(string username)
        {
            return processor.Enqueue(db => db.Users.FirstOrDefaultAsync(e => e.Username == username));
        }

        public Task SaveAsync(User entity)
        {
            return processor.Enqueue(db =>
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
            });
        }
    }
}
