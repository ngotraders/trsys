using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Infrastructure;

namespace Trsys.Web.Infrastructure.ReadModel.Database
{
    public class SqlServerLogDatabase : ILogDatabase, IDisposable
    {

        private readonly ITrsysReadModelContext db;

        public SqlServerLogDatabase(ITrsysReadModelContext db)
        {
            this.db = db;
        }

        public Task AddRangeAsync(IEnumerable<LogDto> logs)
        {
            db.Logs.AddRange(logs);
            return db.SaveChangesAsync();
        }

        public async Task<IEnumerable<LogDto>> SearchAsync(string source, int page, int perPage)
        {
            var query = db.Logs as IQueryable<LogDto>;
            if (!string.IsNullOrEmpty(source))
            {
                query = query.Where(q => q.Key == source);
            }
            if (page > 1)
            {
                query = query.Skip((page - 1) * perPage);
            }
            if (perPage > 0)
            {
                query = query.Take(perPage);
            }
            return await query.OrderByDescending(q => q.Received).ToListAsync();
        }

        public void Dispose()
        {
            db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
