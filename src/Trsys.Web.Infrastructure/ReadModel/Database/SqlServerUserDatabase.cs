using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Infrastructure;

namespace Trsys.Web.Infrastructure.ReadModel.Database
{
    public class SqlServerUserDatabase : IUserDatabase, IDisposable
    {

        private readonly ITrsysReadModelContext db;

        public SqlServerUserDatabase(ITrsysReadModelContext db)
        {
            this.db = db;
        }

        public Task AddAsync(UserDto user)
        {
            db.Users.Add(user);
            return db.SaveChangesAsync();
        }

        public async Task UpdatePasswordHashAsync(Guid id, string passwordHash)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException($"user {id} not found.");
            }
            user.PasswordHash = passwordHash;
            await db.SaveChangesAsync();
        }

        public Task<List<UserDto>> SearchAsync()
        {
            return db.Users.ToListAsync();
        }

        public Task<UserDto> FindByIdAsync(Guid id)
        {
            return db.Users.Where(user => user.Id == id).FirstOrDefaultAsync();
        }

        public Task<UserDto> FindByUsernameAsync(string username)
        {
            return db.Users.Where(user => user.Username == username).FirstOrDefaultAsync();
        }

        public void Dispose()
        {
            db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
