using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
        private TrsysContext db;

        public OrdersApiController(TrsysContext db)
        {
            this.db = db;
        }

        [HttpGet]
        [Produces("text/plain")]
        public async Task<string> GetOrders()
        {
            var orders = await db.Orders.ToListAsync();
            return string.Join("@", orders.Select(o => $"{o.TicketNo}:{o.Symbol}:{(int)o.OrderType}"));
        }

        [HttpPost]
        [Consumes("text/plain")]
        public async Task<IActionResult> PostOrder([FromBody] string text)
        {
            db.Orders.RemoveRange(db.Orders);
            if (!string.IsNullOrEmpty(text))
            {
                var orders = new List<Order>();
                foreach (var item in text.Split("@"))
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
            return Ok();
        }
    }
}
