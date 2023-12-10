using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.Database
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

        public async Task UpdateUserInfoAsync(Guid id, string name, string emailAddress)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException($"user {id} not found.");
            }
            user.Name = name;
            user.EmailAddress = emailAddress;
            await db.SaveChangesAsync();
        }

        public async Task UpdatePasswordHashAsync(Guid id, string passwordHash)
        {
            var userPassword = await db.UserPasswordHashes.FindAsync(id);
            if (userPassword == null)
            {
                db.UserPasswordHashes.Add(new UserPasswordHashDto()
                {
                    Id = id,
                    PasswordHash = passwordHash,
                });
                await db.SaveChangesAsync();
            }
            else
            {
                userPassword.PasswordHash = passwordHash;
                await db.SaveChangesAsync();
            }
        }

        public Task<int> CountAsync()
        {
            return db.Users.CountAsync();
        }

        public Task<List<UserDto>> SearchAsync()
        {
            return db.Users.ToListAsync();
        }

        public Task<List<UserDto>> SearchAsync(int start, int end)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (end <= start)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }
            return db.Users.Skip(start).Take(end - start).ToListAsync();
        }

        public Task<UserDto> FindByIdAsync(Guid id)
        {
            return db.Users.Where(user => user.Id == id).FirstOrDefaultAsync();
        }

        public Task<UserDto> FindByUsernameAsync(string username)
        {
            return db.Users.Where(user => user.Username == username).FirstOrDefaultAsync();
        }

        public Task<UserPasswordHashDto> GetUserPasswordHash(Guid id)
        {
            return db.UserPasswordHashes.Where(user => user.Id == id).FirstOrDefaultAsync();
        }

        public void Dispose()
        {
            db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
