using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.Database
{
    public class SqlServerOrderDatabase : IOrderDatabase, IDisposable
    {
        private readonly ITrsysReadModelContext db;

        public SqlServerOrderDatabase(ITrsysReadModelContext db)
        {
            this.db = db;
        }

        public async Task AddAsync(OrderDto order)
        {
            var dbOrder = await db.Orders
                .Where(o => o.TicketNo == order.TicketNo)
                .FirstOrDefaultAsync();
            if (dbOrder != null)
            {
                return;
            }
            db.Orders.Add(order);
            await db.SaveChangesAsync();
        }

        public async Task<OrdersTextEntry> FindEntryAsync()
        {
            return OrdersTextEntry.Create(await SearchPublishedOrderAsync());
        }

        public async Task RemoveAsync(string id)
        {
            var order = await db.Orders
                .Where(order => order.Id == id)
                .FirstOrDefaultAsync();
            if (order == null)
            {
                return;
            }
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
        }

        public async Task RemoveBySecretKeyAsync(Guid id)
        {
            var orders = await db.Orders.Where(order => order.SecretKeyId == id).ToListAsync();
            db.Orders.RemoveRange(orders);
            await db.SaveChangesAsync();
        }

        public Task<List<OrderDto>> SearchAsync()
        {
            return db.Orders
                .OrderBy(order => order.TicketNo)
                .ToListAsync();
        }

        public Task<List<PublishedOrder>> SearchPublishedOrderAsync()
        {
            return db.Orders
                .OrderBy(order => order.TicketNo)
                .Select(order => order.Order)
                .ToListAsync();
        }
        public void Dispose()
        {
            db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
