using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task UpdateAsync(Guid id, string name, string username, string emailAddress, string role)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException($"user {id} not found.");
            }
            user.Name = name;
            user.Username = username;
            user.EmailAddress = emailAddress;
            user.Role = role;
            await db.SaveChangesAsync();
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

        public Task RemoveAsync(Guid id)
        {
            db.Users.Remove(new UserDto() { Id = id });
            return db.SaveChangesAsync();
        }

        public Task<int> CountAsync()
        {
            return db.Users.CountAsync();
        }

        public Task<List<UserDto>> SearchAsync()
        {
            return db.Users.ToListAsync();
        }

        public Task<List<UserDto>> SearchAsync(int start, int end, string[] sort, string[] order)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (end <= start)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }
            var query = db.Users.AsQueryable();
            if (sort != null && order != null)
            {
                for (var i = 0; i < sort.Length; i++)
                {
                    var sortField = sort[i];
                    var orderField = order[i];
                    if (orderField == "asc")
                    {
                        query = query.OrderBy(GetItemField(sortField));
                    }
                    else if (orderField == "desc")
                    {
                        query = query.OrderByDescending(GetItemField(sortField));
                    }
                }
            }
            return query.Skip(start).Take(end - start).ToListAsync();
        }

        private static Expression<Func<UserDto, object>> GetItemField(string sortField)
        {
            return sortField switch
            {
                "id" => item => item.Id,
                "name" => item => item.Name,
                "username" => item => item.Username,
                "emailAddress" => item => item.EmailAddress,
                "role" => item => item.Role,
                _ => throw new InvalidOperationException($"sort field {sortField} not found."),
            };
        }

        public Task<UserDto> FindByIdAsync(Guid id)
        {
            return db.Users.Where(user => user.Id == id).FirstOrDefaultAsync();
        }

        public Task<UserDto> FindByNormalizedUsernameAsync(string username)
        {
            return db.Users.Where(user => user.Username.ToUpperInvariant() == username.ToUpperInvariant()).FirstOrDefaultAsync();
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
