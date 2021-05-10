using System.Threading.Tasks;
using Trsys.Web.Infrastructure.Generic;
using Trsys.Web.Models.Users;

namespace Trsys.Web.Infrastructure.SQLite
{
    public class SQLiteUserRepository : IUserRepository
    {
        private readonly TrsysContextProcessor processor;

        public SQLiteUserRepository(TrsysContextProcessor processor)
        {
            this.processor = processor;
        }

        public Task<User> FindByUsernameAsync(string username)
        {
            return processor.Enqueue(db => new UserRepository(db).FindByUsernameAsync(username));
        }

        public Task SaveAsync(User entity)
        {
            return processor.Enqueue(db => new UserRepository(db).SaveAsync(entity));
        }
    }
}
