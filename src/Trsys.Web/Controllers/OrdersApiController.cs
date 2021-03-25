using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trsys.Web.Data;
using Trsys.Web.Models;

namespace Trsys.Web.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersApiController : ControllerBase
    {
        private const string ORDERS_HASH = "ORDERS_HASH";

        private readonly TrsysContext db;
        private readonly IMemoryCache cache;

        public OrdersApiController(TrsysContext db, IMemoryCache cache)
        {
            this.db = db;
            this.cache = cache;
        }

        [HttpGet]
        [Produces("text/plain")]
        public async Task<IActionResult> GetOrders()
        {
            var etag = (string)HttpContext.Request.Headers["If-None-Match"];
            if (!string.IsNullOrEmpty(etag))
            {
                if (cache.TryGetValue(ORDERS_HASH, out var cacheEntry))
                {
                    if (etag == $"\"{cacheEntry}\"")
                    {
                        return StatusCode(304);
                    }
                }

            }
            var orders = await db.Orders.ToListAsync();
            var responseText = string.Join("@", orders.Select(o => $"{o.TicketNo}:{o.Symbol}:{(int)o.OrderType}"));
            var hash = CalculateHash(responseText);
            cache.Set(ORDERS_HASH, hash);
            HttpContext.Response.Headers["ETag"] = $"{hash}";
            return Ok(responseText);
        }

        private string CalculateHash(string responseText)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(responseText));
            var str = BitConverter.ToString(hash);
            str = str.Replace("-", string.Empty);
            return str;
        }

        [HttpPost]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostOrder([FromBody] string text)
        {
            db.Orders.RemoveRange(db.Orders);
            if (!string.IsNullOrEmpty(text))
            {
                var orders = new List<Order>();
                foreach (var item in text.Trim(Convert.ToChar(0)).Split("@"))
                {
                    if (!Regex.IsMatch(item, @"^\d+:[A-Z]+:[01]"))
                    {
                        return BadRequest();
                    }
                    var splitted = item.Split(":");
                    var ticketNo = splitted[0];
                    var symbol = splitted[1];
                    var orderType = (OrderType)int.Parse(splitted[2]);
                    orders.Add(new Order() { TicketNo = ticketNo, Symbol = symbol, OrderType = orderType });
                }
                db.Orders.AddRange(orders);
            }
            await db.SaveChangesAsync();
            cache.Remove(ORDERS_HASH);
            return Ok();
        }
    }
}
